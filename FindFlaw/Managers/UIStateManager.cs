using System.Windows.Controls;
using System.Windows.Media;
using FindFlaw.Models;

namespace FindFlaw.Managers
{
    public class UIStateManager
    {
        private bool isAddingLine = false;


        private TextBlock statusLabel;

        private TextBlock lineCountLabel;

        private TextBlock selectedLineIdLabel;

        private TextBox lineLabelTextBox;
        public bool IsAddingLine => isAddingLine;

        public UIStateManager(TextBlock statusLabel, TextBlock lineCountLabel, TextBlock selectedLineIdLabel, TextBox lineLabelTextBox)
        {
            this.statusLabel = statusLabel;
            this.lineCountLabel = lineCountLabel;
            this.selectedLineIdLabel = selectedLineIdLabel;
            this.lineLabelTextBox = lineLabelTextBox;
        }

        
        public void SetStatus(string message)
        {
            statusLabel.Text = message;
        }

        public void UpdateLineCount(int count)
        {
            lineCountLabel.Text = count.ToString();
        }

        public void UpdateSelectedLine(LineMarker? line)
        {
            if (line != null)
            {
                selectedLineIdLabel.Text = line.Id.ToString();
                lineLabelTextBox.Text = line.Label ?? string.Empty;
            }
            else
            {
                selectedLineIdLabel.Text = "None";
                lineLabelTextBox.Text = string.Empty;
            }
        }
    }
}