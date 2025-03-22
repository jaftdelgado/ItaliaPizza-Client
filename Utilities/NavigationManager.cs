using ItaliaPizzaClient.Views;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ItaliaPizzaClient.Utilities
{
    public class NavigationManager
    {
        public List<NavigationItem> navigationStack = new List<NavigationItem>();
        private int currentIndex = -1;
        private Frame mainFrame;
        private StackPanel navigationPanel;
        private Button btnBack;

        public NavigationManager(Frame frame, StackPanel panel, Button backButton)
        {
            mainFrame = frame;
            navigationPanel = panel;
            btnBack = backButton;
            UpdateButtonStates();
        }

        public class NavigationItem
        {
            public string Name { get; set; }
            public Page PageInstance { get; set; }

            public NavigationItem(string name, Page pageInstance)
            {
                Name = name;
                PageInstance = pageInstance;
            }
        }

        public void NavigateToPage(string resourceKey, Page pageInstance)
        {
            string pageName = Application.Current.Resources[resourceKey] as string;

            if (currentIndex < navigationStack.Count - 1)
                navigationStack.RemoveRange(currentIndex + 1, navigationStack.Count - currentIndex - 1);

            navigationStack.Add(new NavigationItem(pageName, pageInstance));
            currentIndex = navigationStack.Count - 1;

            mainFrame.Navigate(pageInstance);
            UpdateNavigationBar();
            UpdateButtonStates();
        }

        public void GoBack()
        {
            if (currentIndex > 0)
            {
                navigationStack.RemoveRange(currentIndex, navigationStack.Count - currentIndex);

                currentIndex--;
                mainFrame.Navigate(navigationStack[currentIndex].PageInstance);
                UpdateNavigationBar();
                UpdateButtonStates();
            }
        }

        private void UpdateNavigationBar()
        {
            navigationPanel.Children.Clear();

            Image principalIcon = new Image
            {
                Style = (Style)mainFrame.FindResource(currentIndex == 0
                    ? "PrincipalIconActiveStyle"
                    : "PrincipalIconStyle")
            };
            navigationPanel.Children.Add(principalIcon);

            for (int i = 0; i < navigationStack.Count; i++)
            {
                var navigationItem = navigationStack[i];

                Button navButton = new Button
                {
                    Content = navigationItem.Name,
                    Tag = navigationItem.PageInstance,
                    Style = (Style)mainFrame.FindResource(i == currentIndex
                        ? "NavigationButtonActiveStyle"
                        : "NavigationButtonStyle")
                };

                navButton.Click += NavButton_Click;
                navigationPanel.Children.Add(navButton);

                if (i < navigationStack.Count - 1)
                    navigationPanel.Children.Add(new Image
                    {
                        Style = (Style)mainFrame.FindResource("NavigationSeparatorStyle")
                    });
            }
        }

        public void ClearNavigationStack()
        {
            navigationStack.Clear();
            NavigateToPage("Glb_Principal", new PrincipalPage());
            currentIndex = 0;

            mainFrame.Navigate(navigationStack[0].PageInstance);
            UpdateNavigationBar();
            UpdateButtonStates();
        }


        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Page targetPage)
            {
                int index = navigationStack.FindIndex(p => p.PageInstance == targetPage);
                if (index >= 0 && index < currentIndex)
                {
                    navigationStack.RemoveRange(index + 1, navigationStack.Count - index - 1);
                    currentIndex = index;
                    mainFrame.Navigate(targetPage);
                    UpdateNavigationBar();
                    UpdateButtonStates();
                }
            }
        }

        private void UpdateButtonStates()
        {
            btnBack.IsEnabled = currentIndex > 0;
        }
    }
}
