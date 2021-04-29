using OfficeOpenXml;
using OfficeOpenXml.Style;
using projektlabor.covid19login.adminpanel.datahandling.entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace projektlabor.covid19login.adminpanel.utils
{
    public class ExcelUtils
    {

        /// <summary>
        /// Applys the default border for the table to the given cell's
        /// </summary>
        /// <param name="cells">The cells to apply the style to</param>
        /// <param name="backgroundColor">The background-color for the table-style</param>
        public static void ApplyTableStyle(ExcelRange cells, int backgroundColor = 0xaeaaaa)
        {
            // Styles the cell(s)
            cells.Style.Border.BorderAround(ExcelBorderStyle.Thick);
            cells.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cells.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(backgroundColor | (0xff << 24)));
        }

        /// <summary>
        /// Displays the default table with a title
        /// </summary>
        /// <param name="sheet">The sheet to displays the table on</param>
        /// <param name="title">Title that will be displayed on the table</param>
        /// <param name="startX">X-start coordinate of the table</param>
        /// <param name="startY">Y-start coordinate of the table</param>
        /// <param name="rows">Amount of rows</param>
        /// <param name="columns">Amount fo columns</param>
        /// <param name="cellSupplier">Callback to supply every single cell of the table</param>
        /// <param name="backgroundColor">Background-color</param>
        public static void PrintBasicTable(ExcelWorksheet sheet, string title, int startX, int startY, int rows, int columns, Action<int, int, ExcelRange> cellSupplier,
                int backgroundColor = 0xaeaaaa)
        {

            // Increments the sheet position to allow starting from 0 (Like in a real programming language)
            startX++;
            startY++;

            // Gets the field bounds
            int width = 2 + 2 * columns;
            int height = 5 + rows;

            // Gets the full field
            var field = sheet.Cells[startY, startX, startY + height - 1, startX + width - 1];

            // Applys the style and border
            ApplyTableStyle(field, backgroundColor);

            // Gets the title field
            var fieldTitle = sheet.Cells[startY + 1, startX + 1, startY + 1 + 1, startX + width - 2];

            // Styles the title field
            fieldTitle.Merge = true;
            fieldTitle.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            fieldTitle.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            fieldTitle.Value = title;
            fieldTitle.Style.Font.Bold = true;
            fieldTitle.Style.Font.Size = 20;

            // Styles the fields
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < columns; c++)
                {
                    // Gets the cell
                    var cell = sheet.Cells[startY + 4 + r, startX + 1 + c * 2, startY + 4 + r, startX + 2 + c * 2];
                    cell.Merge = true;

                    // Applies the cell-supplier
                    cellSupplier(c, r, cell);
                }
        }

        /// <summary>
        /// Styles and sets a hyperlink to the given cells that links to another sheet on the document
        /// </summary>
        /// <param name="cells">The cells to apply to</param>
        /// <param name="sheet">The sheet to link to</param>
        public static void LinkTo(ExcelRange cells, ExcelWorksheet sheet)
        {
            cells.Hyperlink = new Uri("#'" + sheet.Name + "'!A1", UriKind.Relative);
            cells.Style.Font.UnderLine = true;
            cells.Style.Font.Color.SetColor(Color.Blue);
        }

        /// <summary>
        /// Prints a table that is horizontally orientated
        /// </summary>
        public static void PrintHorizontalTable(ExcelWorksheet sheet, string title, int startX, int startY, object[] header, int columns, Action<int, int, ExcelRange> onSupplyValue)
        {
            // Creates the basic table
            PrintBasicTable(sheet, title, startX, startY, header.Length, columns + 1, SupplyCell);

            // Supplyer for every cell
            void SupplyCell(int x, int y, ExcelRange cell)
            {
                // Checks for the header
                if (x == 0)
                {
                    // Appends the header
                    cell.Value = header[y];

                    // Sets the style
                    cell.Style.Font.Bold = true;
                }
                else
                {
                    // Appends the style
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Medium);
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(Color.White);
                }
                // Executes the callback
                onSupplyValue?.Invoke(x, y, cell);
            }
        }

        /// <summary>
        /// Prints a table that is vertically orientated
        /// </summary>
        public static void PrintVerticalTable(ExcelWorksheet sheet, string title, int startX, int startY, object[] header, int rows, Action<int, int, ExcelRange> onSupplyValue)
        {
            // Creates the basic table
            PrintBasicTable(sheet, title, startX, startY, 1 + rows, header.Length, StyleCell);

            // Supplyer for every cell
            void StyleCell(int x, int y, ExcelRange cell)
            {
                // Check for the header
                if (y == 0)
                {
                    // Inserts the value
                    cell.Value = header[x];

                    // Style
                    cell.Style.Font.Bold = true;
                }
                else
                {
                    // Appends the style
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Medium);
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(y % 2 == 0 ? Color.GhostWhite : Color.White);
                }

                // Executes the callback
                onSupplyValue?.Invoke(x, y, cell);
            }
        }

    }
}
