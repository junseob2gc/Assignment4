using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace FTPApp.Models
{
    class CreateXlsx
    {
        public static void CreateSpreadsheetWorkbook(string filepath)
        {
            // Create a spreadsheet document by supplying the filepath.
            // By default, AutoSave = true, Editable = true, and Type = xlsx.
            SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.
                Create(filepath, SpreadsheetDocumentType.Workbook);

            // Add a WorkbookPart to the document.
            WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
            workbookpart.Workbook = new Workbook();

            // Add a WorksheetPart to the WorkbookPart.
            WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());

            // Add Sheets to the Workbook.
            Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.
                AppendChild<Sheets>(new Sheets());

            // Append a new worksheet and associate it with the workbook.
            //Sheet sheet = new Sheet() { Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Sheet 1" };

            //sheets.Append(sheet);

            //workbookpart.Workbook.Save();

            // Close the document.
            spreadsheetDocument.Close();
        }

        // Given a document name and text, 
        // inserts a new work sheet and writes the text to cell "A1" of the new worksheet.

        public static void InsertText(string docName, string text, string colName="A", uint rowIndex=1)
        {
            // Open the document for editing.
            using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Open(docName, true))
            {
                // Get the SharedStringTablePart. If it does not exist, create a new one.
                SharedStringTablePart shareStringPart;
                if (spreadSheet.WorkbookPart.GetPartsOfType<SharedStringTablePart>().Count() > 0)
                {
                    shareStringPart = spreadSheet.WorkbookPart.GetPartsOfType<SharedStringTablePart>().First();
                }
                else
                {
                    shareStringPart = spreadSheet.WorkbookPart.AddNewPart<SharedStringTablePart>();
                }
                


                // Insert a new worksheet.
                WorksheetPart worksheetPart = InsertWorksheet(spreadSheet.WorkbookPart);

                // Insert the text into the SharedStringTablePart.
                int index = InsertSharedStringItem(text, shareStringPart);

                // Insert cell A1 into the new worksheet.
                Cell cell = InsertCellInWorksheet(colName, rowIndex, worksheetPart);

                // Set the value of cell A1.
                cell.CellValue = new CellValue(index.ToString());
                cell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
                
                // Save the new worksheet.
                worksheetPart.Worksheet.Save();
            }
        }

        public static void InsertObject(string docName, List<Student> students)
        {
            
            // Open the document for editing.
            using (SpreadsheetDocument spreadSheet = SpreadsheetDocument.Open(docName, true))
            {
                // Get the SharedStringTablePart. If it does not exist, create a new one.
                SharedStringTablePart shareStringPart;
                if (spreadSheet.WorkbookPart.GetPartsOfType<SharedStringTablePart>().Count() > 0)
                {
                    shareStringPart = spreadSheet.WorkbookPart.GetPartsOfType<SharedStringTablePart>().First();
                }
                else
                {
                    shareStringPart = spreadSheet.WorkbookPart.AddNewPart<SharedStringTablePart>();
                }

                // Insert a new worksheet.
                WorksheetPart worksheetPart = InsertWorksheet(spreadSheet.WorkbookPart);

                // Insert the text into the SharedStringTablePart.
                int[] headers = new int[]
                { InsertSharedStringItem("UniqueId", shareStringPart)
                ,InsertSharedStringItem("StudentId", shareStringPart)
                ,InsertSharedStringItem("FirstName", shareStringPart)
                ,InsertSharedStringItem("LastName", shareStringPart)
                ,InsertSharedStringItem("DateOfBirth", shareStringPart)
                ,InsertSharedStringItem("IsMe", shareStringPart)
                ,InsertSharedStringItem("Age", shareStringPart)};


                List<Cell> headerCells = new List<Cell>();
                uint rowIndex = 1;
                for (int i = 0; i < headers.Length; i++)
                {
                    headerCells.Add(InsertCellInWorksheet(((char)(65+i)).ToString(), rowIndex, worksheetPart));
                }
                int headerCol = 0;
                foreach (var hcell in headerCells)
                {
                    hcell.CellValue = new CellValue(headers[headerCol].ToString());
                    hcell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
                    // Save the new worksheet.
                    worksheetPart.Worksheet.Save();
                    headerCol++;
                }


                foreach (var student in students)
                {
                    rowIndex++;
                    // Insert the text into the SharedStringTablePart.
                    int index = InsertSharedStringItem(student.StudentId, shareStringPart);
                    int[] rows = new int[]
                        { InsertSharedStringItem(Guid.NewGuid().ToString(), shareStringPart)
                         ,InsertSharedStringItem(student.StudentId, shareStringPart)
                         ,InsertSharedStringItem(student.FirstName, shareStringPart)
                         ,InsertSharedStringItem(student.LastName, shareStringPart)
                         ,InsertSharedStringItem(student.DateOfBirth, shareStringPart)
                         ,InsertSharedStringItem(student.IsMe.ToString(), shareStringPart)
                         ,InsertSharedStringItem($"=INT((TODAY()-E{rowIndex})/365)", shareStringPart)};
                        // ,InsertSharedStringItem(student.Age.ToString(), shareStringPart)};

                    List<Cell> valueCells = new List<Cell>();
                    
                    for (int i = 0; i < headers.Length; i++)
                    {
                        valueCells.Add(InsertCellInWorksheet(((char)(65 + i)).ToString(), rowIndex, worksheetPart));
                    }

                    int valueCol = 0;
                    foreach (var vcell in valueCells)
                    {
                        vcell.CellValue = new CellValue(rows[valueCol].ToString());
                        vcell.DataType = new EnumValue<CellValues>(CellValues.SharedString);
                        // Save the new worksheet.
                        worksheetPart.Worksheet.Save();
                        valueCol++;
                    }
                }
                //worksheetPart.Worksheet.Save();
            }
        }

        // Given text and a SharedStringTablePart, creates a SharedStringItem with the specified text 
        // and inserts it into the SharedStringTablePart. If the item already exists, returns its index.
        private static int InsertSharedStringItem(string text, SharedStringTablePart shareStringPart)
        {
            // If the part does not contain a SharedStringTable, create one.
            if (shareStringPart.SharedStringTable == null)
            {
                shareStringPart.SharedStringTable = new SharedStringTable();
            }

            int i = 0;

            // Iterate through all the items in the SharedStringTable. If the text already exists, return its index.
            foreach (SharedStringItem item in shareStringPart.SharedStringTable.Elements<SharedStringItem>())
            {
                if (item.InnerText == text)
                {
                    return i;
                }

                i++;
            }

            // The text does not exist in the part. Create the SharedStringItem and return its index.
            shareStringPart.SharedStringTable.AppendChild(new SharedStringItem(new DocumentFormat.OpenXml.Spreadsheet.Text(text)));
            shareStringPart.SharedStringTable.Save();

            return i;
        }

        // Given a WorkbookPart, inserts a new worksheet.
        private static WorksheetPart InsertWorksheet(WorkbookPart workbookPart)
        {
            // Add a new worksheet part to the workbook.
            WorksheetPart newWorksheetPart = workbookPart.AddNewPart<WorksheetPart>();
            newWorksheetPart.Worksheet = new Worksheet(new SheetData());
            newWorksheetPart.Worksheet.Save();

            Sheets sheets = workbookPart.Workbook.GetFirstChild<Sheets>();
            string relationshipId = workbookPart.GetIdOfPart(newWorksheetPart);

            // Get a unique ID for the new sheet.
            uint sheetId = 1;
            if (sheets.Elements<Sheet>().Count() > 0)
            {
                sheetId = sheets.Elements<Sheet>().Select(s => s.SheetId.Value).Max() + 1;
            }

            string sheetName = "Sheet" + sheetId;

            // Append the new worksheet and associate it with the workbook.
            Sheet sheet = new Sheet() { Id = relationshipId, SheetId = sheetId, Name = sheetName };
            sheets.Append(sheet);
            workbookPart.Workbook.Save();

            return newWorksheetPart;
        }

        // Given a column name, a row index, and a WorksheetPart, inserts a cell into the worksheet. 
        // If the cell already exists, returns it. 
        private static Cell InsertCellInWorksheet(string columnName, uint rowIndex, WorksheetPart worksheetPart)
        {
            Worksheet worksheet = worksheetPart.Worksheet;
            SheetData sheetData = worksheet.GetFirstChild<SheetData>();
            string cellReference = columnName + rowIndex;

            // If the worksheet does not contain a row with the specified row index, insert one.
            Row row;
            if (sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).Count() != 0)
            {
                row = sheetData.Elements<Row>().Where(r => r.RowIndex == rowIndex).First();
            }
            else
            {
                row = new Row() { RowIndex = rowIndex };
                sheetData.Append(row);
            }

            // If there is not a cell with the specified column name, insert one.  
            if (row.Elements<Cell>().Where(c => c.CellReference.Value == columnName + rowIndex).Count() > 0)
            {
                return row.Elements<Cell>().Where(c => c.CellReference.Value == cellReference).First();
            }
            else
            {
                // Cells must be in sequential order according to CellReference. Determine where to insert the new cell.
                Cell refCell = null;
                foreach (Cell cell in row.Elements<Cell>())
                {
                    if (string.Compare(cell.CellReference.Value, cellReference, true) > 0)
                    {
                        refCell = cell;
                        break;
                    }
                }

                Cell newCell = new Cell() { CellReference = cellReference };
                row.InsertBefore(newCell, refCell);

                worksheet.Save();
                return newCell;
            }
        }

       
    }
}
