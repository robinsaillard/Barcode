using System;
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
                                        typeof(object),
                                        typeof(RichTextBox),
                                        new PropertyMetadata(
                                            new PropertyChangedCallback
                                                (RichTextPropertyChanged)));

        public RichTextViewer()
        {
            IsReadOnly = true;
            Background = new SolidColorBrush { Opacity = 0 };
            BorderThickness = new Thickness(0);
            IsDocumentEnabled = true;
        }

        public object RichText
        {
            get { return (object)GetValue(RichTextProperty); }
            set { SetValue(RichTextProperty, value); }
        }

        private static void RichTextPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            Paragraph paragraph = new Paragraph()
            {
                Margin = new Thickness(0)
            };
            paragraph.Inlines.Add(new Run("[" + DateTime.Now.ToLongTimeString() + "]: "));
            if (dependencyPropertyChangedEventArgs.NewValue is string @string)
            {             
                paragraph.Inlines.Add(new Run(string.Format(@string)));
                ((RichTextBox)dependencyObject).Document.Blocks.Add(paragraph);
            }
            if(dependencyPropertyChangedEventArgs.NewValue is Paragraph paragraphVal)
            {
                ((RichTextBox)dependencyObject).Document.Blocks.Add(paragraphVal);
            }
            if(dependencyPropertyChangedEventArgs.NewValue is Run run)
            {
                paragraph.Inlines.Add(run);
                ((RichTextBox)dependencyObject).Document.Blocks.Add(paragraph);
            }
        }
    }
}
