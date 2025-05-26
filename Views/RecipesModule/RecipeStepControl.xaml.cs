using System.Windows;
using System.Windows.Controls;

namespace ItaliaPizzaClient.Views.RecipesModule
{
    public partial class RecipeStepControl : UserControl
    {
        public static readonly DependencyProperty StepNumberProperty =
            DependencyProperty.Register("StepNumber", typeof(int), typeof(RecipeStepControl),
                new PropertyMetadata(0, OnStepNumberChanged));

        public static readonly DependencyProperty InstructionProperty =
            DependencyProperty.Register("Instruction", typeof(string), typeof(RecipeStepControl),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public event RoutedEventHandler DeleteRequested;

        public event RoutedEventHandler InstructionChanged;

        public int StepNumber
        {
            get => (int)GetValue(StepNumberProperty);
            set => SetValue(StepNumberProperty, value);
        }

        public string Instruction
        {
            get => (string)GetValue(InstructionProperty);
            set => SetValue(InstructionProperty, value);
        }

        public RecipeStepControl()
        {
            InitializeComponent();
        }

        private static void OnStepNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RecipeStepControl step)
            {
                step.StepNumberText.Text = $"{step.StepNumber}.";
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteRequested?.Invoke(this, e);
        }

        private void InstructionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                Instruction = tb.Text;
                InstructionChanged?.Invoke(this, new RoutedEventArgs());
            }
        }
    }
}
