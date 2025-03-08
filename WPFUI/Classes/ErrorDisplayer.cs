using System;
using System.Windows;

namespace WPFUI;

public static class ErrorDisplayer
{
    public static void ShowErrorOnWindowOpening(Exception ex)
    {
        MessageBox.Show($"Error while opening Window:{Environment.NewLine}{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
    public static void ShowErrorOnWindowInitializing(Exception ex)
    {
        MessageBox.Show($"Error while initalizing Window:{Environment.NewLine}{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
    public static void ShowErrorOnAction(string openingMessage, Exception ex)
    {
        MessageBox.Show($"{openingMessage}:{Environment.NewLine}{ex}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
