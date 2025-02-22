using System;
using System.Windows;

namespace InvisibleManXRay
{
    public partial class PolicyWindow : Window
    {
        private Action onEmailClick;

        public PolicyWindow()
        {
            InitializeComponent();
        }

        public void Setup(Action onEmailClick)
        {
            this.onEmailClick = onEmailClick;
        }

        private void OnEmailClick(object sender, RoutedEventArgs e)
        {
            onEmailClick.Invoke();
        }
    }
}