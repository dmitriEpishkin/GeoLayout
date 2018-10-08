using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace GeoLayout.ViewElements {
    public class LatLongTextBox : TextBox {
        public const string LatTemplate = "90º 60' 60.00\" N";
        public const string LonTemplate = "180º 60' 60.00\" E";

        private bool _handlingInternalSelection; //to prevent recursion when selection event causes a change in selection
        private bool _movingLeft;

        private class Field {
            public int FieldStart;
            public int FieldLength;
            public double MaxValue;
        }

        private readonly List<Field> _fields = new List<Field>();

        private const string OK_CHARACTERS_NO_DOT = "0123456789NE";
        private const string OK_DIGITS_AND_DOT = "0123456789.";

        private const string ERROR_MESSAGE = "Неверный символ";
        private const string VALUE_MUST_BE_LESS_THAN = "Значение должно быть меньше чем ";

        private string _inputTemplate = "";

        private bool _textOrValueChanging;

        public string InputTemplate {
            get { return _inputTemplate; }
            // when the inputTemplate is set, it is parsed into fields
            set {
                _fields.Clear();
                _inputTemplate = value;
                // find the input fields-delimited by any character which is not a digit or '.'
                int charPos = 0;
                while (charPos < _inputTemplate.Length - 1) {
                    // skip over label characters between fields
                    while (charPos < _inputTemplate.Length && !OK_DIGITS_AND_DOT.Contains(_inputTemplate[charPos]))
                        charPos++;
                    if (charPos < _inputTemplate.Length - 1) {
                        var f = new Field {
                            FieldStart = charPos
                        };
                        while (charPos < _inputTemplate.Length && OK_DIGITS_AND_DOT.Contains(_inputTemplate[charPos]))
                            charPos++;
                        f.FieldLength = charPos - f.FieldStart;
                        double.TryParse(_inputTemplate.Substring(f.FieldStart, f.FieldLength), NumberStyles.Float, CultureInfo.InvariantCulture, out f.MaxValue);
                        _fields.Add(f);
                    }
                }

                // find E/W or N/S fields
                int ind = _inputTemplate.IndexOfAny(new[] { 'E', 'N' });
                if (ind != -1) {
                    _fields.Add(new Field {
                        FieldStart = ind,
                        FieldLength = 1,
                        MaxValue = -1
                    });
                }

                // WTF !!!!
                //все потому что вся логика тут не умеет работать с пустым полем ввода, 
                //алсо, надо поддерживать текст в соответствии с шаблоном иначе кровькишкиэксепшн
                base.Text = GetTextFromValue(this.Value.GetValueOrDefault()); // update the text with the new template
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double?), typeof(LatLongTextBox), new PropertyMetadata(default(double?), ValueChangedCallback));

        private static void ValueChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs) {
            var control = dependencyObject as LatLongTextBox;
            var newValue = (double?)dependencyPropertyChangedEventArgs.NewValue;
            if (control == null || !newValue.HasValue) // зачем второе условие?
                return;

            if (control._textOrValueChanging)
                return;
            try {
                control._textOrValueChanging = true;

                // body
                var textFromValue = control.GetTextFromValue(newValue.Value);
                control.Text = textFromValue;
                // !body

            }
            finally {
                control._textOrValueChanging = false;
            }
        }

        public double? Value {
            get { return (double?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        protected override void OnTextChanged(TextChangedEventArgs e) {
            if (_textOrValueChanging)
                return;
            try {
                _textOrValueChanging = true;

                // body
                var valueFromText = GetValueFromText(Text);
                Value = valueFromText;
                // !body

            }
            finally {
                _textOrValueChanging = false;
            }
        }

        private string GetTextFromValue(double val) {
            var sign = val > 0;
            val = Math.Abs(val);

            double minutes = (val - Math.Floor(val)) * 60;
            double seconds = (minutes - Math.Floor(minutes)) * 60;

            var d = Math.Floor(val);
            var m = Math.Floor(minutes);
            var s = seconds;

            var isLat = _inputTemplate == LatTemplate;

            return string.Format("{0:" + (isLat ? "00" : "000") + "}º {1:00}' {2:00.00}\" {3}", d, m, s, sign ? isLat ? 'N' : 'E' : isLat ? 'S' : 'W');
        }

        private double GetValueFromText(string text) {
            var lngOffset = _inputTemplate == LatTemplate ? 0 : 1;
            int d = int.Parse(text.Substring(0, 2 + lngOffset));
            int m = int.Parse(text.Substring(4 + lngOffset, 2));
            var s = double.Parse(text.Substring(8 + lngOffset, 5));

            s = Math.Round(s, 2);

            var val = d + m / 60d + s / 3600d;

            if (text.Last() == 'S' || text.Last() == 'W')
                val *= -1;

            return Math.Round(val, 7);
        }

        public LatLongTextBox() {
            CharacterCasing = CharacterCasing.Upper;

            AllowDrop = false;
            ContextMenu = null;

            PreviewKeyDown += TextBoxTemplate_PreviewKeyDown;
        }

        //if the control got focus via a keyboard TAB, then position the cursor appropriately
        protected override void OnGotFocus(RoutedEventArgs e) {
            if (Keyboard.Modifiers == ModifierKeys.Shift) {
                //position at the end of the box
                Field lastField = _fields[_fields.Count - 1];
                CaretIndex = lastField.FieldStart + lastField.FieldLength - 1;
            }
            else if (Mouse.LeftButton == MouseButtonState.Released) {
                //position at the beginnning of the box
                CaretIndex = 0;
            }
        }

        protected override void OnSelectionChanged(RoutedEventArgs e) {
            if (!_handlingInternalSelection) {
                _handlingInternalSelection = true;

                var lastField = _fields.Last();
                while (SelectionStart > 0 && SelectionStart >= lastField.FieldStart + lastField.FieldLength)
                    SelectionStart--;
                if (!_movingLeft && SelectionStart < base.Text.Length - 1 && !OK_CHARACTERS_NO_DOT.Contains(_inputTemplate[SelectionStart]))
                    SelectionStart = _inputTemplate.IndexOfAny(OK_CHARACTERS_NO_DOT.ToCharArray(), SelectionStart);
                else if (_movingLeft && SelectionStart > 0) {
                    SelectionStart--;
                    var prevIndex = PreviousIndexOfAny(_inputTemplate, OK_CHARACTERS_NO_DOT.ToCharArray(), SelectionStart);
                    if (prevIndex != -1)
                        SelectionStart = prevIndex;
                }

                if (SelectionStart > base.Text.Length - 1 && Text.Length > 0)
                    SelectionStart = base.Text.Length - 1;
                SelectionLength = 1;

                _handlingInternalSelection = false;
            }
            _movingLeft = false;
        }

        private static int PreviousIndexOfAny(string str, char[] anyOf, int startIndex) {
            while (Array.IndexOf(anyOf, str[startIndex]) == -1 && startIndex > 0)
                startIndex--;
            return startIndex;
        }

        private bool ValueInRange(Key key, out string max) {
            bool valid = false;
            max = "";
            if (key >= Key.D0 && key <= Key.D9) {
                int digit = key - Key.D0;
                Field theField = null;
                foreach (var f in _fields) {
                    if (SelectionStart >= f.FieldStart && SelectionStart < f.FieldStart + f.FieldLength) {
                        theField = f;
                        break;
                    }
                }
                var sb = new StringBuilder(Text.Substring(theField.FieldStart, theField.FieldLength));
                sb[SelectionStart - theField.FieldStart] = (char)('0' + (char)digit);
                double val;
                double.TryParse(sb.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out val);
                if (val < theField.MaxValue || theField.MaxValue == 0)
                    valid = true;
                else { //will zeroing the subsequent digit make it ligit?
                    if (SelectionStart - theField.FieldStart + 1 < sb.Length) {
                        int index = SelectionStart - theField.FieldStart + 1;
                        sb[index] = '0';
                        double.TryParse(sb.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out val);
                        if (val < theField.MaxValue) {
                            base.Text = base.Text.Remove(theField.FieldStart, theField.FieldLength).Insert(theField.FieldStart, sb.ToString());
                            valid = true;
                        }
                    }
                }
                max = theField.MaxValue.ToString();
            }
            return valid;
        }

        private void TextBoxTemplate_PreviewKeyDown(object sender, KeyEventArgs e) {
            string errorMessage = ERROR_MESSAGE;
            bool valid = false;

            Key key = e.Key;

            //remap numeric keypad to regular keys for validation
            if (key >= Key.NumPad0 && key <= Key.NumPad9)
                key = key - Key.NumPad0 + Key.D0;

            //check for value out-of-range
            string maxValue;
            if (ValueInRange(key, out maxValue))
                valid = true;
            else if (key >= Key.D0 && key <= Key.D9 && char.IsDigit(_inputTemplate[SelectionStart]))
                errorMessage = VALUE_MUST_BE_LESS_THAN + maxValue;

            switch (key) {
                case Key.Down:
                case Key.Up:
                    valid = true;
                    break;

                case Key.Right:
                    //allow the right cursor key
                    //if you're at the last character, replace with a TAB
                    Field lastField = _fields[_fields.Count - 1];
                    int lastPostion = lastField.FieldStart + lastField.FieldLength - 1;
                    if (SelectionStart == lastPostion) {
                        e.Handled = true;
                        return;
                    }
                    valid = true;
                    break;

                case Key.Left:
                    if (SelectionStart == _fields[0].FieldStart) {
                        e.Handled = true;
                        return;
                    }
                    _movingLeft = true;
                    valid = true;
                    break;

                case Key.Decimal:
                case Key.OemPeriod:
                    int index = _inputTemplate.IndexOf('.');
                    if (index > -1) {
                        CaretIndex = index + 1;
                        valid = true;
                        e.Handled = true;
                    }
                    break;

                case Key.E:
                case Key.W:
                    if (_inputTemplate[SelectionStart] == 'E')
                        valid = true;
                    break;

                case Key.N:
                case Key.S:
                    if (_inputTemplate[SelectionStart] == 'N')
                        valid = true;
                    break;

                //case Key.Back:
                //case Key.Delete:
                //    errorMessage = "Overstrike characters. Deletion is not needed.";
                //    break;

                case Key.C:
                    if (Keyboard.Modifiers != ModifierKeys.Control)
                        break;
                    Clipboard.SetDataObject(Value, true);
                    valid = true;
                    e.Handled = true;
                    break;

                case Key.V:
                    if (Keyboard.Modifiers != ModifierKeys.Control)
                        break;
                    var obj = Clipboard.GetDataObject();
                    if (obj != null) {
                        var val = obj.GetData(typeof(double));
                        if (val != null)
                            Value = (double)val;
                    }
                    valid = true;
                    e.Handled = true;
                    break;

                case Key.Escape:
                case Key.System:
                case Key.Return:
                case Key.LeftAlt:
                case Key.RightShift:
                case Key.LeftShift:
                case Key.Tab:
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    valid = true;
                    break;

            }

            if (!valid)
                ShowError(e, errorMessage);
        }

        private void ShowError(KeyEventArgs e, string errorMessage) {
            //use a popup for the error message
            var pp = new Popup();
            ToolTipService.SetShowDuration(pp, 50);
            var tb = new TextBlock {
                Text = errorMessage,
                Foreground = SystemColors.InfoTextBrush,
                Background = SystemColors.InfoBrush
            };

            var b1 = new Border { Child = tb };
            pp.Child = b1;
            b1.BorderThickness = new Thickness(1);
            b1.BorderBrush = SystemColors.ActiveBorderBrush;

            pp.Height = 20;
            pp.PlacementTarget = this;
            pp.StaysOpen = false;
            Rect r = GetRectFromCharacterIndex(SelectionStart);
            pp.HorizontalOffset = r.Left;
            pp.IsOpen = true;

            var dt = new DispatcherTimer {
                Interval = new TimeSpan(0, 0, 1, 300)
            };

            dt.Tick += delegate {
                pp.IsOpen = false;
                dt.Stop();
            };
            dt.Start();

            e.Handled = true;
            System.Media.SystemSounds.Beep.Play();
        }
    }
}
