using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Nordwest;

namespace Nordwest.TimeSeries {
    public static class TimeSeriesJoinUtil {

        public static void RunJoining(List<ITimeSeries<float>> ts, int mtuCount, int skip, double gapPart) {
            ts.Sort((x, y) => x.StartTime.CompareTo(y.StartTime));

            // Объединить перекрывающиеся ряды и ряды следующие один за другим без пробела
            for (int i = 1; i < ts.Count; i++) {
                if (ts[i].StartTime <= ts[i - 1].StartTime + TimeSpan.FromSeconds(ts[i - 1].Data.Count / ts[i - 1].SampleRate)) {
                    // объединить в одно целое
                    ts[i - 1] = Join(ts[i - 1], ts[i]);
                    ts.RemoveAt(i);
                    i--;
                }
            }

            //заполнить небольшие промежутки 
            Combine(ts, mtuCount, skip, gapPart);

        }

        public static ITimeSeries<float> Join(ITimeSeries<float> t1, ITimeSeries<float> t2) {

            Requires.Argument(Equals(t1.SampleRate, t2.SampleRate));

            if (t1.StartTime > t2.StartTime) {
                var t = t1;
                t1 = t2;
                t2 = t;
            }

            var t2Start = t2.StartTime;
            var t1End = t1.StartTime + TimeSpan.FromSeconds(t1.Data.Count / t1.SampleRate);

            Contract.Assert(t2Start <= t1End, "Между рядами существует пробел");

            // определить величину перекрытия в отсчетах
            var span = (int)((t1End - t2Start).TotalSeconds * t1.SampleRate);

            // если полное перекрытие, то не берём лишнего
            if (span > t2.Data.Count)
                span = t2.Data.Count;

            var d = new float[t1.Data.Count + t2.Data.Count - span];

            var i1 = t1.Data.Count - span;

            var i1Span = t1.Data.Count;

            // если полное перекрытие, то второй цикл перезаписывает (!) центр. третий - не выполняется

            for (int i = 0; i < i1; i++) d[i] = t1.Data[i];
            for (int i = 0; i < span; i++) d[i1 + i] = (t1.Data[i1 + i] + t2.Data[i]) / 2.0f;
            for (int i = span; i < t2.Data.Count; i++) d[i1Span + i] = t2.Data[i];

            return new TimeSeriesBase(t1.SampleRate, t1.StartTime, d);
        }

        public static void Combine(List<ITimeSeries<float>> tt, int mtuCount, int skip, double gapPart) {

            // сортировка по времени
            tt.Sort((x, y) => x.StartTime.CompareTo(y.StartTime));
            // кусочечный ряд
            if (tt.Count > mtuCount * 2) FillGrid(tt);
            // непреывный ряд
            else Fill(tt, skip, gapPart);

        }

        /// <summary>
        /// Равномерно дополняет сетку временных рядов с постоянным шагом
        /// </summary>
        public static void FillGrid(List<ITimeSeries<float>> tt) {
            // определяем шаг
            var s = new List<TimeSpan>();
            for (int i = 1; i < tt.Count; i++)
                s.Add(tt[i].StartTime - tt[i - 1].StartTime);

            s.Sort();
            var step = s[s.Count / 2];
            // длительность (в отсчетах)
            var ds = tt.ConvertAll(q => q.Data.Count);
            ds.Sort();
            var len = ds[tt.Count / 2];

            var rnd = new Random(42);

            int count = 0;
            while (count < tt.Count - 1) {

                // недописки
                if (tt[count + 1].Data.Count < len) {
                    if (tt[count + 1].StartTime - tt[count].StartTime == step) {
                        var f = new float[len];
                        for (int i = 0; i < tt[count + 1].Data.Count; i++)
                            f[i] = tt[count + 1].Data[i];
                        for (int i = tt[count + 1].Data.Count; i < len; i++) {
                            var ind = tt[count + 1].Data.Count - i;
                            f[i] = tt[count + 1].Data[ind < 0 ? rnd.Next(tt[count + 1].Data.Count) : ind];
                        }
                        tt[count + 1] = new TimeSeriesBase(tt[count + 1].SampleRate, tt[count + 1].StartTime, f);
                    }
                    else if (tt[count + 1].StartTime - tt[count].StartTime > step) {
                        var f = new float[len];
                        // величина недописки
                        var start = len - tt[count + 1].Data.Count;

                        for (int i = 0; i < start; i++) {
                            var ind = i;
                            f[i] =
                                tt[count + 1].Data[
                                    ind >= tt[count + 1].Data.Count ? rnd.Next(tt[count + 1].Data.Count) : ind];
                        }
                        for (int i = start; i < len; i++)
                            f[i] = tt[count + 1].Data[i - start];

                        tt[count + 1] = new TimeSeriesBase(tt[count + 1].SampleRate, tt[count].StartTime + step, f);
                    }

                }

                // проверка на пропуск кусков
                if (tt[count + 1].StartTime - tt[count].StartTime > step) {
                    // вставляем сегмент
                    // если следующий кусок есть, то

                    // определяем количество пропусков
                    int skipN =
                        (int)Math.Truncate(((tt[count + 1].StartTime - tt[count].StartTime).TotalSeconds / step.TotalSeconds - 1));

                    for (int w = 0; w < skipN; w++) {

                        var f = new float[len];

                        // если слева и справа достаточно
                        if (count > skipN && skipN < tt.Count - count) {
                            for (int a = 0; a < len; a++) {
                                f[a] = ((skipN - w) / (float)skipN) *
                                       tt[count - w].Data[
                                           a < tt[count - w].Data.Count ? a : rnd.Next(tt[count - w].Data.Count)]
                                       +
                                       (w + 1) / (float)skipN *
                                       tt[count + 1 + skipN].Data[
                                           len - a < tt[count + 1 + skipN].Data.Count
                                               ? len - a
                                               : rnd.Next(tt[count + 1 + skipN].Data.Count)];
                            }
                        }
                        // случайный выбор кусков
                        else {
                            int c1 = rnd.Next(count);
                            int c2 = rnd.Next(count, tt.Count);
                            for (int a = 0; a < len; a++) {
                                f[a] = 0.5f * tt[c1].Data[
                                           a < tt[c1].Data.Count ? a : rnd.Next(tt[c1].Data.Count)]
                                       +
                                       0.5f *
                                       tt[c2].Data[
                                           len - a < tt[c2].Data.Count
                                               ? len - a
                                               : rnd.Next(tt[c2].Data.Count)];
                            }
                        }

                        tt.Insert(count + 1 + w,
                                  new TimeSeriesBase(tt[count].SampleRate,
                                                     tt[count].StartTime +
                                                     TimeSpan.FromSeconds(step.TotalSeconds * (1 + w)), f));
                    }
                }
                else {
                    count++;
                }
            }
        }

        public static void Fill(List<ITimeSeries<float>> tt, int skip, double gapPart) {
            // если начала и окончания рядов шумные, 
            // можно обрезать слева-справа по некоторому количеству дискретов
            for (int i = 0; i < tt.Count - 1; i++) {
                if (tt[i].Data.Count > skip * 20) {
                    var ttt = new float[tt[i].Data.Count - skip];
                    for (int x = 0; x < ttt.Length; x++)
                        ttt[x] = tt[i].Data[x];
                    tt[i] = new TimeSeriesBase(tt[i].SampleRate, tt[i].StartTime, ttt);
                }
                if (tt[i + 1].Data.Count > skip * 20) {
                    var ttt = new float[tt[i + 1].Data.Count - skip];
                    for (int x = 0; x < ttt.Length; x++)
                        ttt[x] = tt[i + 1].Data[skip + x];
                    tt[i + 1] = new TimeSeriesBase(tt[i + 1].SampleRate, tt[i + 1].StartTime.AddSeconds(10), ttt);
                }
            }

            // посчитать длину ряда
            var len =
                (int)((tt[tt.Count - 1].StartTime.AddSeconds(tt[tt.Count - 1].Data.Count / tt[tt.Count - 1].SampleRate) -
                        tt[0].StartTime).TotalSeconds * tt[0].SampleRate);
            var d = new float[len];

            int cur = 0;

            for (int i = 0; i < tt.Count; i++) {
                for (int j = 0; j < tt[i].Data.Count; j++) {
                    d[cur] = tt[i].Data[j];
                    cur++;
                }
                if (cur == len) break;
                // вычислить паузу
                var lenGap =
                    (int)
                    ((tt[i + 1].StartTime - tt[i].StartTime.AddSeconds(tt[i].Data.Count / tt[i].SampleRate)).TotalSeconds *
                     tt[i].SampleRate);

                // заполнить паузу

                // если промежуток маленький
                if (lenGap < gapPart * tt[i].Data.Count && lenGap < gapPart * tt[i + 1].Data.Count) {
                    for (int j = 0; j < lenGap; j++) {
                        d[cur] = (float)((lenGap - j) / (double)lenGap * tt[i].Data[tt[i].Data.Count - 1 - j]
                                          + (j + 1) / (double)lenGap * tt[i + 1].Data[lenGap - 1 - j]);
                        cur++;
                    }
                }
                // иначе заполнить средним
                else {
                    var avg = (tt[i].Data.Average() + tt[i + 1].Data.Average()) / 2.0f;
                    for (int j = 0; j < lenGap; j++) {
                        d[cur] = avg;
                        cur++;
                    }
                }
            }
            var newTs = new TimeSeriesBase(tt[0].SampleRate, tt[0].StartTime, d);
            tt.Clear();
            tt.Add(newTs);
        }
        
    }
}
