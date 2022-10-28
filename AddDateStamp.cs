using Ascon.Pilot.SDK.Menu;
using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Xml.Serialization;
using Ascon.Pilot.SDK.ObjectsSample;
using AddDateStamp.Properties;
using System.Data;

namespace Ascon.Pilot.SDK.GraphicLayerSample
{
      
    [Export(typeof(IMenu<XpsRenderClickPointContext>))]
    public class AddDateStamp : IMenu<XpsRenderClickPointContext>
    {
        private readonly IObjectsRepository _repository;
        private DataObjectWrapper _selected;
        private readonly IObjectModifier _modifier;
        private readonly IPerson _currentPerson;
        private double _xOffset;
        private double _yOffset;
        private int _pageNumber;
        private VerticalAlignment _verticalAlignment;
        private HorizontalAlignment _horizontalAlignment;
        private bool notFrozen = false;
        //private bool transparentBackground = true;
        private AccessLevel _accessLevel = AccessLevel.None;
        private const string AddDateStampMenuItem = "AddDateStampMenuItem";
        private string text = "";
        private string fontSize = "20";
        private string fontFamilyName = "Times New Roman";
        private System.Drawing.Color textColor = System.Drawing.Color.Black;

        [ImportingConstructor]
        public AddDateStamp(IObjectModifier modifier, IObjectsRepository repository)
        {
            _modifier = modifier;
            _currentPerson = repository.GetCurrentPerson();
            _repository = repository;  

        }


        public void Build(IMenuBuilder builder, XpsRenderClickPointContext context)
        //создание пунктов меню: "Перенести подпись сюда" и "Перенести сюда и повернуть":
        {
            //запрос прав на согласование документа:
            _selected = new DataObjectWrapper(context.DataObject, _repository);
            _accessLevel = GetMyAccessLevel(_selected);
            notFrozen = !(_selected.StateInfo.State.ToString().Contains("Frozen"));

            builder.AddItem(AddDateStampMenuItem, 0)
                   .WithHeader(Resources.AddDateStampMenuItem)
                   .WithIsEnabled((((int)_accessLevel & 16) != 0) & notFrozen); //пункт меню активен, если есть право согласовывать и документ не заморожен
        }

        public void OnMenuItemClick(string name, XpsRenderClickPointContext context)
        {

            if (name == AddDateStampMenuItem)
            {
                _pageNumber = context.PageNumber + 1; //задание номера страницы
                _xOffset = (context.ClickPoint.X) * 25.4 / 96; //установка координат в точку клика мышом
                _yOffset = (context.ClickPoint.Y) * 25.4 / 96;
                DateEditoeView DateEditoeView = new DateEditoeView();
                DateEditoeView.ShowDialog();
                if (!DateEditoeView.cancel)
                {
                    //transparentBackground = DateEditoeView.transparentBackground;
                   // fontFamilyName = DateEditoeView.fontFamilyName;
                    textColor = DateEditoeView.textColor;
                    fontSize = DateEditoeView.fontSize;
                    if (!int.TryParse(fontSize, out int intFontSize))
                        intFontSize = 12;
                    text = DateEditoeView.selectedDateStr/*.Replace("\n", "<LineBreak />") относилось к печати в виде XAML*/;
                    if (text != "")
                        AddDateStampTextElement(context.DataObject, text, intFontSize);
                }
            }

                       
        }


        private void AddDateStampTextElement(IDataObject dataObject, string text, int intFontSize)
        {
            var elementId = Guid.NewGuid(); // рандомный GUID
            System.Drawing.Image textImage = TextToImage(text, fontFamilyName, intFontSize * 4, textColor); //рисование текста в bitmap
            /* втавка текста в виде текста (устарело):
            string xamlObject1 = XElement.Parse(string.Format("<TextBlock Foreground=\"Black\" FontSize=\"" + fontSize + "\" TextAlignment=\"Left\">" + text + "</TextBlock>")).ToString();
            SaveToDataBaseXaml(dataObject, xamlObject1, elementId);
            */
            SaveToDataBaseTextBitmap(dataObject, textImage, elementId);
        }


        private void SaveToDataBaseTextBitmap(IDataObject dataObject, System.Drawing.Image textBitmap, Guid elementId)
        {
            IObjectBuilder objectBuilder = _modifier.Edit(dataObject);
            int position = _currentPerson.MainPosition.Position;
            MemoryStream memoryStream1 = new MemoryStream();
            textBitmap.Save(memoryStream1, System.Drawing.Imaging.ImageFormat.Png);
            Point scale = new Point(0.25, 0.25);
            string name = "PILOT_GRAPHIC_LAYER_ELEMENT_" + elementId + "_" + position; //имя файла с записью свойств картинки
                                                                                       //ПРИВЯЗАНО К ЧЕЛОВЕКУ В ВИДЕ _currentPerson.MainPosition.Position в конце имени файла
            GraphicLayerElement o = GraphicLayerElementCreator.Create(_xOffset, _yOffset, scale, 0, position, _verticalAlignment, _horizontalAlignment, "bitmap", elementId, _pageNumber, true);
            using (MemoryStream memoryStream2 = new MemoryStream())
            {
                new XmlSerializer(typeof(GraphicLayerElement)).Serialize(memoryStream2, o);
                objectBuilder.AddFile(name, memoryStream2, DateTime.Now, DateTime.Now, DateTime.Now); //создание записи о расположении картинки на листе
                objectBuilder.AddFile("PILOT_CONTENT_GRAPHIC_LAYER_ELEMENT_" + o.ContentId, memoryStream1, DateTime.Now, DateTime.Now, DateTime.Now); //создание файла PNG. НЕ СОДЕРЖИТ ПРИВЯЗКУ К ЧЕЛОВЕКУ.
                                                                                                                                                      //CONTENT ID - РАНДОМНЫЙ GUID
            }
            _modifier.Apply();

        }

        /* Сохранение текста в виде XAML (устарело):
        private void SaveToDataBaseXaml(IDataObject dataObject, string xamlObject, Guid elementId)
        {
            IObjectBuilder objectBuilder = _modifier.Edit(dataObject);
            MemoryStream memoryStream1 = new MemoryStream();
            using (StreamWriter streamWriter = new StreamWriter(memoryStream1))
            {
                streamWriter.Write(xamlObject);
                streamWriter.Flush();
                int position = _currentPerson.MainPosition.Position;
                string name = "PILOT_GRAPHIC_LAYER_ELEMENT_" + elementId + "_" + position; //имя файла записи с рандомным elementID и привязкой к человеку через ID пользователя
                GraphicLayerElement o = GraphicLayerElementCreator.Create(_xOffset, _yOffset, new Point(_scaleXY, _scaleXY), _angle, position, _verticalAlignment, _horizontalAlignment, "xaml", elementId, _pageNumber, true);
                using (MemoryStream memoryStream2 = new MemoryStream())
                {
                    new XmlSerializer(typeof(GraphicLayerElement)).Serialize(memoryStream2, o);
                    objectBuilder.AddFile(name, memoryStream2, DateTime.Now, DateTime.Now, DateTime.Now);
                }
                objectBuilder.AddFile("PILOT_CONTENT_GRAPHIC_LAYER_ELEMENT_" + o.ContentId, memoryStream1, DateTime.Now, DateTime.Now, DateTime.Now); //создаёт файл с текстом XAML с рандомным именем.
                                                                                                                                                      //PILOT_CONTENT_GRAPHIC_LAYER_ELEMENT_ в распакованном XPS не найден.
                                                                                                                                                      //contentId всегда рандомный
                _modifier.Apply();
            }
        }
        */


        private System.Drawing.Image TextToImage(string text, string fontName, int intFontSize, System.Drawing.Color textColor)
            //рисовалка текста, чтобы отвязаться от установленных у пользователей шрифтов
        {
            System.Drawing.Font font = new System.Drawing.Font(fontName, intFontSize);
            //System.Drawing.Font handWriteFont = new System.Drawing.Font(fontCollection.Families[0], intFontSize);

            //first, create a dummy bitmap just to get a graphics object
            System.Drawing.Image img = new System.Drawing.Bitmap(1, 1);
            System.Drawing.Graphics drawing = System.Drawing.Graphics.FromImage(img);

            //DirectoryInfo d = new DirectoryInfo("/");
            //FileInfo[] fileArray = d.GetFiles("*.ttf");
            //string str = "";
            //foreach (FileInfo file in fileArray)
            //    str = str + file.DirectoryName + "\n";
            //text = str;


            //measure the string to see how big the image needs to be
            //if (fontName == "Katherine Plus")
            //{
            //    byte[] fontData = Resources.KatherinePlus;
            //    IntPtr fontPtr = System.Runtime.InteropServices.Marshal.AllocCoTaskMem(fontData.Length);
            //    System.Runtime.InteropServices.Marshal.Copy(fontData, 0, fontPtr, fontData.Length);
            //    System.Drawing.Text.PrivateFontCollection fontCollection = new System.Drawing.Text.PrivateFontCollection();
            //    fontCollection.AddMemoryFont(fontPtr, Resources.KatherinePlus.Length);
            //    System.Runtime.InteropServices.Marshal.FreeCoTaskMem(fontPtr);
            //    font = new System.Drawing.Font(fontCollection.Families[0], intFontSize);
            //}
            System.Drawing.SizeF textSize = drawing.MeasureString(text, font);

            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            //create a new image of the right size
            img = new System.Drawing.Bitmap((int)textSize.Width, (int)textSize.Height);
            drawing = System.Drawing.Graphics.FromImage(img);

            //create a brush for the text
            //if (!transparentBackground)
            //    drawing.Clear(System.Drawing.Color.White); //adding white background
            System.Drawing.Brush textBrush = new System.Drawing.SolidBrush(textColor);
            //drawing.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; //не оправдало себя
            //drawing.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias; //не оправдало себя
            drawing.DrawString(text, font, textBrush, 0, 0);
            drawing.Save();
            textBrush.Dispose();
            drawing.Dispose();
            return img;
        }




        private AccessLevel GetMyAccessLevel(DataObjectWrapper element)
        {
            var currentAccesLevel = AccessLevel.None;
            var person = _repository.GetCurrentPerson();
            foreach (var position in person.AllOrgUnits())
            {
                currentAccesLevel |= GetAccessLevel(element, position);
            }

            return currentAccesLevel;
        }

        private AccessLevel GetAccessLevel(DataObjectWrapper element, int positonId)
        {
            var currentAccesLevel = AccessLevel.None;
            var orgUnits = _repository.GetOrganisationUnits().ToDictionary(k => k.Id);
            var accesses = GetAccessRecordsForPosition(element, positonId, orgUnits);
            foreach (var source in accesses)
            {
                currentAccesLevel |= source.Access.AccessLevel;
            }
            return currentAccesLevel;
        }

        private IEnumerable<AccessRecordWrapper> GetAccessRecordsForPosition(DataObjectWrapper obj, int positionId, IDictionary<int, IOrganisationUnit> organisationUnits)
        {
            return obj.Access.Where(x => BelongsTo(positionId, x.OrgUnitId, organisationUnits));
        }

        public static bool BelongsTo(int position, int organisationUnit, IDictionary<int, IOrganisationUnit> organisationUnits)
        {
            Stack<int> units = new Stack<int>();
            units.Push(organisationUnit);
            while (units.Any())
            {
                var unitId = units.Pop();
                if (position == unitId)
                    return true;

                IOrganisationUnit unit;
                if (organisationUnits.TryGetValue(unitId, out unit))
                {
                    foreach (var childUnitId in unit.Children)
                    {
                        units.Push(childUnitId);
                    }
                }
            }
            return false;
        }
    }
}
