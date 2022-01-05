using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace BarcodeReader.Controls
{
    public class RichTextViewer : RichTextBox
    {
        public const string RichTextPropertyName = "RichText";

        public static readonly DependencyProperty RichTextProperty =
            DependencyProperty.Register(RichTextPropertyName,
                                        typeof(string),
                                        typeof(RichTextBox),
                                        new PropertyMetadata(
                                            new PropertyChangedCallback
                                                (RichTextPropertyChanged)));

        public RichTextViewer()
        {
            IsReadOnly = true;
            Background = new SolidColorBrush { Opacity = 0 };
            BorderThickness = new Thickness(0);
        }

        public string RichText
        {
            get { return (string)GetValue(RichTextProperty); }
            set { SetValue(RichTextProperty, value); }
        }

        private static void RichTextPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(new Run(string.Format((string)dependencyPropertyChangedEventArgs.NewValue)));
            ((RichTextBox)dependencyObject).Document.Blocks.Add(paragraph);

        }
    }
}
