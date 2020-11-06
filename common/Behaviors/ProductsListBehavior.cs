#region Copyright Syncfusion Inc. 2001-2020.
// Copyright Syncfusion Inc. 2001-2020. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Microsoft.Xaml.Behaviors;
using Syncfusion.SfSkinManager;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace syncfusion.demoscommon.wpf
{
    public class ProductsListBehavior : Behavior<ProductsListView>
    {
        /// <summary>
        /// Maintains view model refference
        /// </summary>
        private DemoBrowserViewModel sampleBrowserViewModel;

        /// <summary>
        /// Maintains scroll viewer refference
        /// </summary>
        private ScrollViewer scrollViewer;

        /// <summary>
        /// Called after the behavior is attached to an AssociatedObject.
        /// </summary>
        protected override void OnAttached()
        {
            AssociatedObject.Loaded += new System.Windows.RoutedEventHandler(AssociatedObject_Loaded);
        }

        /// <summary>
        /// Handles the Loaded event of the AssociatedObject control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        void AssociatedObject_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            sampleBrowserViewModel = AssociatedObject.DataContext as DemoBrowserViewModel;
            AssociatedObject.LinkedInlink.RequestNavigate += LinkedInlink_RequestNavigate;
            AssociatedObject.youtubelink.RequestNavigate += LinkedInlink_RequestNavigate;
            AssociatedObject.twitterlink.RequestNavigate += LinkedInlink_RequestNavigate;
            AssociatedObject.facebooklink.RequestNavigate += LinkedInlink_RequestNavigate;
            AssociatedObject.Documentation.RequestNavigate += LinkedInlink_RequestNavigate;
            AssociatedObject.github.RequestNavigate += LinkedInlink_RequestNavigate;
            AssociatedObject.NavigateForward.Click += NavigateForward_Click;
            AssociatedObject.NavigateBackward.Click += NavigateBackward_Click;
            AssociatedObject.ShowcaseList.SelectionChanged += ShowcaseList_SelectionChanged;
            scrollViewer = Syncfusion.Windows.Shared.VisualUtils.FindDescendant(AssociatedObject.ShowcaseList, typeof(ScrollViewer)) as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
                var totalWidth = AssociatedObject.ShowcaseList.Items.Count * 308;
                if (totalWidth <= scrollViewer.ViewportWidth)
                {
                    AssociatedObject.NavigateForward.IsEnabled = false;
                }
            }
        }

        /// <summary>
        /// Occurs when the selection of show case demos
        /// </summary>
        private void ShowcaseList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            if (listView != null && listView.SelectedItem is DemoInfo && sampleBrowserViewModel != null)
            {
                try
                {
                    DemoInfo demo = listView.SelectedItem as DemoInfo;
                    if (demo.ShowBusyIndicator)
                    {
                        sampleBrowserViewModel.IsShowCaseDemoBusy = true;
                    }
                    var window = Activator.CreateInstance(demo.DemoViewType) as Window;
                    AssociatedObject.ShowcaseList.SelectedItem = null;

                    DemosNavigationService.MainWindow.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        sampleBrowserViewModel.IsShowCaseDemoBusy = false;
                    }),
                    System.Windows.Threading.DispatcherPriority.ApplicationIdle);

                    if (window != null)
                    {
                        window.Owner = DemosNavigationService.MainWindow;
                        window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                        window.Closed += Window_Closed;
                        window.ShowDialog();
                    }
                }
                catch (Exception exception)
                {
                    sampleBrowserViewModel.IsShowCaseDemoBusy = false;
                    ErrorWindow.Show(exception.Message + "\n" + exception.StackTrace);
                }
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            (sender as Window).Closed -= Window_Closed;
            (sender as Window).Owner = null;
        }

        /// <summary>
        /// Occurs when the previous Showcase Demos button is pressed.
        /// </summary>
        private void NavigateBackward_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (scrollViewer != null)
            {
                Animate(scrollViewer, -Math.Floor(scrollViewer.ViewportWidth / 2));
            }
        }

        /// <summary>
        /// Occurs when changes are detected to the scroll position, extent, or viewport  size.
        /// </summary>
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var target = sender as ScrollViewer;

            if ((e.HorizontalOffset + e.ViewportWidth) == e.ExtentWidth)
            {
                AssociatedObject.NavigateForward.IsEnabled = false;
            }
            else
            {
                AssociatedObject.NavigateForward.IsEnabled = true;
            }
            if (target.HorizontalOffset > 0)
            {
                AssociatedObject.NavigateBackward.IsEnabled = true;
            }
            else
            {
                AssociatedObject.NavigateBackward.IsEnabled = false;
            }
        }

        /// <summary>
        /// Occurs when the next Showcase Demos button is pressed.
        /// </summary>
        private void NavigateForward_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (scrollViewer != null)
            {
                Animate(scrollViewer, Math.Floor(scrollViewer.ViewportWidth / 2));
            }
        }

        /// <summary>
        /// Helps to perform showcase demo scrolling animation
        /// </summary>
        private void Animate(ScrollViewer target, double speed)
        {
            double startOffset = target.HorizontalOffset;
            double animationTime = 2;
            Stopwatch startTime = new Stopwatch();
            startTime.Start();
            EventHandler renderHandler = null;
            renderHandler = (sender, args) =>
            {
                double elapsed = startTime.Elapsed.TotalSeconds;

                if (elapsed >= animationTime)
                {
                    CompositionTarget.Rendering -= renderHandler;
                    startTime.Stop();
                }

                target.ScrollToHorizontalOffset(startOffset + (elapsed * speed));
            };
            CompositionTarget.Rendering += renderHandler;
        }

        /// <summary>
        ///  Occurs when navigation events are requested.
        /// </summary>
        private void LinkedInlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            var process = new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            };
            Process.Start(process);
            e.Handled = true;
        }

        /// <summary>
        /// Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.
        /// </summary>
        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= new System.Windows.RoutedEventHandler(AssociatedObject_Loaded);
            AssociatedObject.LinkedInlink.RequestNavigate -= LinkedInlink_RequestNavigate;
            AssociatedObject.youtubelink.RequestNavigate -= LinkedInlink_RequestNavigate;
            AssociatedObject.twitterlink.RequestNavigate -= LinkedInlink_RequestNavigate;
            AssociatedObject.facebooklink.RequestNavigate -= LinkedInlink_RequestNavigate;
            AssociatedObject.Documentation.RequestNavigate -= LinkedInlink_RequestNavigate;
            AssociatedObject.github.RequestNavigate -= LinkedInlink_RequestNavigate;
            AssociatedObject.NavigateForward.Click -= NavigateForward_Click;
            AssociatedObject.NavigateBackward.Click -= NavigateBackward_Click;
            AssociatedObject.ShowcaseList.SelectionChanged -= ShowcaseList_SelectionChanged;
            scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
            if(scrollViewer != null)
            {
                scrollViewer = null;
            }
        }
    }
}
