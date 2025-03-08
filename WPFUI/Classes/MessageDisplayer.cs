using System;
using System.Windows;

namespace WPFUI;

public static class MessageDisplayer
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
    public static void ShowMessage(string openingMessage)
    {
        MessageBox.Show($"{openingMessage}", "Info", MessageBoxButton.OK, MessageBoxImage.Exclamation);
    }
    public static void ShowErrorMessage(string openingMessage, Exception ex)
    {
        MessageBox.Show($"{openingMessage}:{Environment.NewLine}{Environment.NewLine}{ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
