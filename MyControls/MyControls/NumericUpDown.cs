using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyControls
{
    [TemplatePart(Name = "View_TextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "View_IncreaseButton", Type = typeof(RepeatButton))]
    [TemplatePart(Name = "View_DecraeseButton", Type = typeof(RepeatButton))]
    public class NumericUpDown : Control
    {
        static NumericUpDown()
        {
            EventManager.RegisterClassHandler(typeof(NumericUpDown), Mouse.MouseDownEvent, new MouseButtonEventHandler(NumericUpDown.OnMouseLeftButtonDown), true);

            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(typeof(NumericUpDown)));

        }
        private const double DefaultMinimum = 0;
        private const double DefaultValue = DefaultMinimum;
        private const double DefaultMaximum = 100;
        private const double DefaultStep = 1;

        private TextBox TextBox;
        private RepeatButton IncreaseButton;
        private RepeatButton DecreaseButton;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("View_TextBox") is TextBox textBox)
            {
                TextBox = textBox;
                TextBox.LostFocus += TextBox_LostFocus;
                TextBox.PreviewTextInput += OnPreviewTextInput;
                TextBox.TextInput += TextBox_TextInput;

                TextBox.InputBindings.Add(new KeyBinding(_increaseValueCommand, new KeyGesture(Key.Up)));
                TextBox.InputBindings.Add(new KeyBinding(_decreaseValueCommand, new KeyGesture(Key.Down)));
            }

            if (GetTemplateChild("View_IncreaseButton") is RepeatButton increaseButton)
            {
                IncreaseButton = increaseButton;
                IncreaseButton.Focusable = false;
                IncreaseButton.Command = _increaseValueCommand;
                IncreaseButton.PreviewMouseLeftButtonDown += (sender, args) => RemoveFocus();
            }

            if (GetTemplateChild("View_DecraeseButton") is RepeatButton decreaseButton)
            {
                DecreaseButton = decreaseButton;
                DecreaseButton.Focusable = false;
                DecreaseButton.Command = _decreaseValueCommand;
                IncreaseButton.PreviewMouseLeftButtonDown += (sender, args) => RemoveFocus();
            }

            CommandBindings.Add(new CommandBinding(_increaseValueCommand, (a, b) => IncreaseValue_Click()));
            CommandBindings.Add(new CommandBinding(_decreaseValueCommand, (a, b) => DecreaseValue_Click()));
            CommandBindings.Add(new CommandBinding(_updateValueStringCommand, (a, b) =>
            {
                Value = TryTakeNumberFromTextbox(TextBox.Text);
                RemoveFocus();
            }));

            CommandManager.RegisterClassInputBinding(typeof(TextBox), new KeyBinding(_updateValueStringCommand, new KeyGesture(Key.Enter)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(_increaseValueCommand, new KeyGesture(Key.Up)));
            CommandManager.RegisterClassInputBinding(typeof(TextBox), new InputBinding(_decreaseValueCommand, new KeyGesture(Key.Down)));
        }


        private void RemoveFocus()
        {
            Focusable = true;
            Focus();
            Focusable = false;
        }

        private void CheckInputText()
        {
            if(double.TryParse(TextBox.Text, out _))
            {
                Value = double.Parse(TextBox.Text);
            }
            else
            {
                SystemSounds.Hand.Play();
                TextBox.Text = Minimum.ToString();
            }
        }

        //------------Properties--------------
        #region ---------------PROPERTIES-------------------------

        public double Value
        {
            get => Math.Round((double)GetValue(ValueProperty),2);
            set { SetValue(ValueProperty, value); }
        }

        private static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value",
                                        typeof(double),
                                        typeof(NumericUpDown),
                                        new PropertyMetadata(DefaultValue, new PropertyChangedCallback(OnValueChanged), new CoerceValueCallback(CoerceValue)));
        #endregion
        private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            NumericUpDown numericUpDown = sender as NumericUpDown;
            double oldValue = (double)args.OldValue;
            double newValue = (double)args.NewValue;
            RoutedPropertyChangedEventArgs<double> arguments = new RoutedPropertyChangedEventArgs<double>(oldValue, newValue, ValueChangedEvent);

            numericUpDown.OnValueChanged(arguments);
        }

        public static readonly RoutedEvent ValueChangedEvent =
                 EventManager.RegisterRoutedEvent("ValueChanged", RoutingStrategy.Direct, typeof(RoutedPropertyChangedEventHandler<double>), typeof(NumericUpDown));

        public event RoutedPropertyChangedEventHandler<double> ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        protected virtual void OnValueChanged(RoutedPropertyChangedEventArgs<double> arguments)
        {
            RaiseEvent(arguments);
        }

        private static object CoerceValue(DependencyObject sender, object baseValue)
        {
            NumericUpDown numericUpDown = sender as NumericUpDown;
            double value = (double)baseValue;
            value = Math.Round(Math.Max(numericUpDown.Minimum, Math.Min(numericUpDown.Maximum, value)), numericUpDown.SymbolsAfterDot);

            return value;
        }


        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set { SetValue(MinimumProperty, value); }
        }

        private static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(double), typeof(NumericUpDown), new PropertyMetadata(DefaultMinimum, new PropertyChangedCallback(OnMinValueChanged)));

        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set { SetValue(MaximumProperty, value); }
        }

        private static void OnMinValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs arg)
        {
            NumericUpDown numericUpDown = sender as NumericUpDown;
            double minimum = Convert.ToDouble(arg.NewValue);
            if (minimum > numericUpDown.Maximum)
            {
                numericUpDown.Maximum = minimum;
            }
        }

        private static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(double), typeof(NumericUpDown), new PropertyMetadata(DefaultMaximum, new PropertyChangedCallback(OnMaxValueChanged)));

        private static void OnMaxValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs arg)
        {
            NumericUpDown numericUpDown = sender as NumericUpDown;
            double maximum = Convert.ToDouble(arg.NewValue);
            if (maximum < numericUpDown.Minimum)
            {
                numericUpDown.Minimum = maximum;
            }
        }

        public double Step
        {
            get => (double)GetValue(StepProperty);
            set { SetValue(StepProperty, value); }
        }

        private static readonly DependencyProperty StepProperty =
            DependencyProperty.Register("Step", typeof(double), typeof(NumericUpDown), new PropertyMetadata(DefaultStep));

        [DefaultValue(2)]
        public int SymbolsAfterDot
        {
            get { return (int)GetValue(SymbolsAfterDotProperty); }
            set { SetValue(SymbolsAfterDotProperty, value); }
        }

        private static readonly DependencyProperty SymbolsAfterDotProperty =
            DependencyProperty.Register("SymbolsAfterDot", typeof(int), typeof(NumericUpDown), new FrameworkPropertyMetadata(2,
                    new PropertyChangedCallback(OnSymbolsAfterDotChanged)), new ValidateValueCallback(ValidateSymbolsAfterDot));

        private static void OnSymbolsAfterDotChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            NumericUpDown numericUpDown = sender as NumericUpDown;
            numericUpDown.CoerceValue(ValueProperty);
        }

        private static bool ValidateSymbolsAfterDot(object value)
        {
            int symbolsAfterDot = (int)value;
            return symbolsAfterDot >= 0;
        }

        public bool IsButtonsEnabled
        {
            get { return (bool)GetValue(IsButtonsEnabledProperty); }
            set { SetValue(IsButtonsEnabledProperty, value); }
        }

        private static readonly DependencyProperty IsButtonsEnabledProperty =
            DependencyProperty.Register("IsButtonsEnabled", typeof(bool), typeof(NumericUpDown), new PropertyMetadata(true));

        public bool IsTextBoxEnabled
        {
            get { return (bool)GetValue(IsTextBoxEnabledProperty); }
            set { SetValue(IsTextBoxEnabledProperty, value); }
        }

        private static readonly DependencyProperty IsTextBoxEnabledProperty =
            DependencyProperty.Register("IsTextBoxEnabled", typeof(bool), typeof(NumericUpDown), new PropertyMetadata(true));

        //Properties Color
        public Brush BackColorText
        {
            get { return (Brush)GetValue(BackColorTextProperty); }
            set { SetValue(BackColorTextProperty, value); }
        }

        private static readonly DependencyProperty BackColorTextProperty =
            DependencyProperty.Register("BackColorText", typeof(Brush), typeof(NumericUpDown), new PropertyMetadata(Brushes.White));

        public Brush FontColorText
        {
            get { return (Brush)GetValue(FontColorTextProperty); }
            set { SetValue(FontColorTextProperty, value); }
        }

        private static readonly DependencyProperty FontColorTextProperty =
            DependencyProperty.Register("FontColorText", typeof(Brush), typeof(NumericUpDown), new PropertyMetadata(Brushes.Black));

        public Brush BackColorButtons
        {
            get { return (Brush)GetValue(BackColorButtonsProperty); }
            set { SetValue(BackColorButtonsProperty, value); }
        }

        private static readonly DependencyProperty BackColorButtonsProperty =
            DependencyProperty.Register("BackColorButtons", typeof(Brush), typeof(NumericUpDown), new PropertyMetadata(Brushes.White));

        public Brush FontColorButtons
        {
            get { return (Brush)GetValue(FontColorButtonsProperty); }
            set { SetValue(FontColorButtonsProperty, value); }
        }

        private static readonly DependencyProperty FontColorButtonsProperty =
            DependencyProperty.Register("FontColorButtons", typeof(Brush), typeof(NumericUpDown), new PropertyMetadata(Brushes.Black));


        //----------Commands----------
        //Working with TextBox
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Value = TryTakeNumberFromTextbox(TextBox.Text);
        }

        public double TryTakeNumberFromTextbox(string argument)
        {
            double valueFromTextbox = Minimum;
            double.TryParse(argument, NumberStyles.Float, CultureInfo.InvariantCulture, out valueFromTextbox);
            return valueFromTextbox;
        }

        private readonly RoutedUICommand _updateValueStringCommand =
                new RoutedUICommand("UpdateValueString", "UpdateValueString", typeof(NumericUpDown));

        protected void OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            var fullText = textBox.Text.Remove(textBox.SelectionStart, textBox.SelectionLength).Insert(textBox.CaretIndex, e.Text);
            bool checkError = !double.TryParse(fullText, NumberStyles.Float, CultureInfo.InvariantCulture, out _);
            if (checkError)
            {
                SystemSounds.Hand.Play();
            }
            e.Handled = !double.TryParse(fullText,NumberStyles.Float, CultureInfo.InvariantCulture, out _);
        }

        private void TextBox_TextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            bool checkError = !double.TryParse(textBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out _);
            if (checkError)
            {
                SystemSounds.Hand.Play();
            }
            e.Handled = !double.TryParse(textBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out _);
        }

        //CommandButton
        private readonly RoutedUICommand _increaseValueCommand =
            new RoutedUICommand("IncreaseValueCommand", "IncreaseValueCommand", typeof(NumericUpDown));

        private readonly RoutedUICommand _decreaseValueCommand =
            new RoutedUICommand("DecreaseValueCommand", "DecreaseValueCommand", typeof(NumericUpDown));

        private void IncreaseValue_Click()
        {
            Value += Step;
            if (Value > Maximum)
            {
                Value -= Step;
            }
        }
        private void DecreaseValue_Click()
        {
            Value -= Step;
            if (Value < Minimum)
            {
                Value += Step;
            }
        }

        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NumericUpDown numericUpDown = sender as NumericUpDown;

            if (!numericUpDown.IsKeyboardFocusWithin)
            {
                e.Handled = numericUpDown.Focus() || e.Handled;
            }
        }
    }
}
