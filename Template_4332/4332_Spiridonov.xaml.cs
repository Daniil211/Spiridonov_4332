using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using Excel = Microsoft.Office.Interop.Excel;
using Newtonsoft.Json;
using System.IO;
using System.Data.Entity.Validation;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml;
using System.Data.Entity;

namespace Template_4332
{
    /// <summary>
    /// Логика взаимодействия для _4332_Spiridonov.xaml
    /// </summary>
    public partial class _4332_Spiridonov : System.Windows.Window
    {
        public Excel.Range xlSheetRange;

        public _4332_Spiridonov()
        {
            InitializeComponent();
        }
        #region Импорт лр2
        private void import_Click(object sender, RoutedEventArgs e)
        {
            string[,] list;
            Excel.Application ObjWorkExcel = new Excel.Application();
            Excel.Workbook ObjWorkBook = ObjWorkExcel.Workbooks.Open(@"C:\Users\id202\Desktop\Импорт\2.xlsx");
            Excel.Worksheet ObjWorkSheet = (Excel.Worksheet)ObjWorkBook.Sheets[1];
            var lastCell = ObjWorkSheet.Cells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell);
            int _columns = (int)lastCell.Column;
            int _rows = (int)lastCell.Row;
            list = new string[_rows, _columns];
            for (int j = 0; j < _columns; j++)
                for (int i = 0; i < _rows; i++)
                    list[i, j] = ObjWorkSheet.Cells[i + 1, j + 1].Text;
            ObjWorkBook.Close(false, Type.Missing, Type.Missing);
            ObjWorkExcel.Quit();
            GC.Collect();
            using (ModelContContainer usersEntities = new ModelContContainer())
            {

                for (int i = 1; i < _rows; i++)
                {
                    usersEntities.EntityModelSet.Add(new EntityModel()
                    {
                        Code_zakaza = list[i, 1],
                        Date_create = list[i, 2],
                        Code_client = list[i, 4],
                        Uslugi = list[i, 5]
                    });
                }
                usersEntities.SaveChanges();
                MessageBox.Show("Данные импортированы");
            }
        }
        #endregion
        #region Экспорт лр2
        private void ExportToWorksheet(IEnumerable<EntityModel2> data, Excel.Worksheet ws, string wsName)
        {
            int Row = 1;
            ws.Name = wsName;
            ws.Cells[1][Row] = "Код заказа";
            ws.Cells[2][Row] = "Дата создания";
            ws.Cells[3][Row] = "Время заказа";
            ws.Cells[4][Row] = "Код клиента";
            ws.Cells[5][Row] = "Услуги";
            ws.Cells[6][Row] = "Статус";
            ws.Cells[7][Row] = "Дата закрытия";
            ws.Cells[8][Row] = "Время проката";
            Row++;
            foreach (EntityModel2 item in data)
            {
                ws.Cells[1][Row] = item.CodeZakaza;
                ws.Cells[2][Row] = item.DateCreate;
                ws.Cells[3][Row] = item.TimeCreate;
                ws.Cells[4][Row] = item.CodeClient;
                ws.Cells[5][Row] = item.Uslugi;
                ws.Cells[6][Row] = item.State;
                ws.Cells[7][Row] = item.DateClosed;
                ws.Cells[8][Row] = item.Time_Prokat;
                Row++;
                Excel.Range rangeBorders = ws.Range[ws.Cells[1][1], ws.Cells[4][Row - 1]];
                rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle = rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle = rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle = rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle = rangeBorders.Borders[Excel.XlBordersIndex.xlInsideHorizontal].LineStyle = rangeBorders.Borders[Excel.XlBordersIndex.xlInsideVertical].LineStyle = Excel.XlLineStyle.xlContinuous;
                ws.Columns.AutoFit();
            }
        }
        private void export_Click(object sender, RoutedEventArgs e)
        {
            var app = new Excel.Application();
            app.SheetsInNewWorkbook = 2;

            Excel.Workbook workbook = app.Workbooks.Add(Type.Missing);

            using (ModelExcelContainer usersEntities = new ModelExcelContainer())
            {
                var minutes = usersEntities.EntityModel2Set.Where(p => new[] { "120 минут", "600 минут", "320 минут", "480 минут" }.Contains(p.Time_Prokat));
                ExportToWorksheet(minutes, app.Sheets[1], "Время в минутах");

                var hours = usersEntities.EntityModel2Set.Where(p => new[] { "2 часа", "4 часа", "6 часов", "10 часов", "12 часов" }.Contains(p.Time_Prokat));
                ExportToWorksheet(hours, app.Sheets[2], "Время в часах");
            }

            MessageBox.Show("Файл создан");
            app.Visible = true;

        }
        #endregion
        #region Импорт JSON данных лр3
        public static List<Order> LoadOrdersFromJsonFile(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string json = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<List<Order>>(json);
            }
        }
        public static void SaveOrdersToDatabase(List<Order> orders)
        {
            using (var context = new OrderContext())
            {
                foreach (var order in orders)
                {
                    context.Orders.Add(order);
                }
                try
                {
                    context.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var error in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in error.ValidationErrors)
                        {
                            MessageBox.Show($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                        }
                    }
                }
            }
        }
        private void importJSON_Click(object sender, RoutedEventArgs e)
        {
            string filePath = "C:\\Users\\id202\\Desktop\\3 курс\\ИСРПО\\Импорт\\2.json";
            List<Order> orders = LoadOrdersFromJsonFile(filePath);
            SaveOrdersToDatabase(orders);
            MessageBox.Show("Complete");
        }
        #endregion
        #region Экспорт в Word лр3
        private void exportWord_Click(object sender, RoutedEventArgs e)
        {
            List<Order> data = new List<Order>();
            using (var context = new OrderContext())
            {
                data = context.Orders.ToList();
            }

            var group1 = data.Where(p => new[] { "120 минут" }.Contains(p.ProkatTime));
            var group2 = data.Where(p => new[] { "600 минут" }.Contains(p.ProkatTime));
            var group3 = data.Where(p => new[] { "320 минут" }.Contains(p.ProkatTime));
            var group4 = data.Where(p => new[] { "480 минут" }.Contains(p.ProkatTime));
            var group5 = data.Where(p => new[] { "2 часа" }.Contains(p.ProkatTime));
            var group6 = data.Where(p => new[] { "4 часа" }.Contains(p.ProkatTime));
            var group7 = data.Where(p => new[] { "6 часов" }.Contains(p.ProkatTime));
            var group8 = data.Where(p => new[] { "10 часов" }.Contains(p.ProkatTime));
            var group9 = data.Where(p => new[] { "12 часов" }.Contains(p.ProkatTime));

            string fileName = "output_" + DateTime.Now.ToString("dd.MM.HH.mm.ss") + ".docx";
            using (WordprocessingDocument doc = WordprocessingDocument.Create(fileName, WordprocessingDocumentType.Document))
            {
                if (doc.MainDocumentPart == null)
                {
                    doc.AddMainDocumentPart();
                }
                if (doc.MainDocumentPart.Document == null)
                {
                    doc.MainDocumentPart.Document = new Document();
                }
                Body body = new Body();
                SectionProperties sectionProperties = new SectionProperties();
                body.Append(sectionProperties);

                doc.MainDocumentPart.Document.Body = body;
                //
                Paragraph text = new Paragraph(new Run(new Text("120 минут:")));
                body.Append(text);

                Table table1 = CreateTable(group1);
                body.Append(table1);

                Paragraph para = new Paragraph(new Run(new Break() { Type = BreakValues.Page }));
                body.Append(para);
                //
                Paragraph text2 = new Paragraph(new Run(new Text("600 минут:")));
                body.Append(text2);

                Table table2 = CreateTable(group2);
                body.Append(table2);

                Paragraph para2 = new Paragraph(new Run(new Break() { Type = BreakValues.Page }));
                body.Append(para2);
                //
                Paragraph text11 = new Paragraph(new Run(new Text("320 минут:")));
                body.Append(text11);

                Table table11= CreateTable(group3);
                body.Append(table11);

                Paragraph para11 = new Paragraph(new Run(new Break() { Type = BreakValues.Page }));
                body.Append(para11);
                //
                Paragraph text12 = new Paragraph(new Run(new Text("480 минут:")));
                body.Append(text12);

                Table table12 = CreateTable(group4);
                body.Append(table12);

                Paragraph para12 = new Paragraph(new Run(new Break() { Type = BreakValues.Page }));
                body.Append(para12);
                //
                Paragraph text13 = new Paragraph(new Run(new Text("2 часа :")));
                body.Append(text13);

                Table table14 = CreateTable(group5);
                body.Append(table14);

                Paragraph para15 = new Paragraph(new Run(new Break() { Type = BreakValues.Page }));
                body.Append(para15);
                //
                Paragraph text16 = new Paragraph(new Run(new Text("4 часа:")));
                body.Append(text16);

                Table table16 = CreateTable(group6);
                body.Append(table16);

                Paragraph para16 = new Paragraph(new Run(new Break() { Type = BreakValues.Page }));
                body.Append(para16);
                //
                Paragraph text17 = new Paragraph(new Run(new Text("6 часов:")));
                body.Append(text17);

                Table table17 = CreateTable(group7);
                body.Append(table17);

                Paragraph para17 = new Paragraph(new Run(new Break() { Type = BreakValues.Page }));
                body.Append(para17);
                //
                Paragraph text18 = new Paragraph(new Run(new Text("10 часов:")));
                body.Append(text18);

                Table table19 = CreateTable(group8);
                body.Append(table19);

                Paragraph para19 = new Paragraph(new Run(new Break() { Type = BreakValues.Page }));
                body.Append(para19);
                //
                Paragraph text20 = new Paragraph(new Run(new Text("12 часов:")));
                body.Append(text20);

                Table table20 = CreateTable(group9);
                body.Append(table20);

                Paragraph para20 = new Paragraph(new Run(new Break() { Type = BreakValues.Page }));
                body.Append(para20);
                //



                Paragraph dates = new Paragraph(new Run(new Text("Дата первого заказа: " + data.Min(p => DateTime.Parse(p.CreateDate))
                    .ToString("dd.MM.yyyy") + ", дата последнего заказа: " + data.Max(p => DateTime
                    .Parse(p.CreateDate)).ToString("dd.MM.yyyy"))));
                body.Append(dates);

                doc.MainDocumentPart.Document.Save();
            }
            MessageBox.Show("Complete");
        }
        static Table CreateTable(IEnumerable<Order> data)
        {
            Table table = new Table();

            TableRow headerRow = new TableRow();
            headerRow.Append(new TableCell(new Paragraph(new Run(new Text("Id")))));
            headerRow.Append(new TableCell(new Paragraph(new Run(new Text("Код заказа")))));
            headerRow.Append(new TableCell(new Paragraph(new Run(new Text("Дата создания")))));
            headerRow.Append(new TableCell(new Paragraph(new Run(new Text("Код клиента")))));
            headerRow.Append(new TableCell(new Paragraph(new Run(new Text("Услуги")))));
            table.Append(headerRow);

            foreach (Order item in data)
            {
                TableRow row = new TableRow();
                row.Append(new TableCell(new Paragraph(new Run(new Text(item.Id.ToString())))));
                row.Append(new TableCell(new Paragraph(new Run(new Text(item.CodeOrder)))));
                row.Append(new TableCell(new Paragraph(new Run(new Text(item.CreateDate + " " + item.CreateTime))))); 
                row.Append(new TableCell(new Paragraph(new Run(new Text(item.CodeClient)))));
                row.Append(new TableCell(new Paragraph(new Run(new Text(item.Services)))));
                table.Append(row);
            }

            return table;
        }
#endregion
    }
}
