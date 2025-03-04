namespace FlowProtocol2.Core
{
    public class DocumentBuilder
    {
        public OMDocument Document { get; private set; }
        public string CurrentSection { get; set; }
        private OMTextBlock? CurrentTextBlock { get; set; }
        private OMTextLine? CurrentTextline { get; set; }
        private int IDCounter { get; set; }

        public DocumentBuilder()
        {
            Document = new OMDocument();
            CurrentSection = string.Empty;
            IDCounter = 0;
        }
        public void SetTitle(string title)
        {
            Document.Title = title;
        }
        /// <summary>
        /// Fügt eine neue Textzeile zum Dokument hinzu
        /// </summary>
        /// <param name="l">Level, auf dem die Zeile hinzugefügt werden soll (1 oder 2).</param>
        /// <param name="t">Typ des Textblocks</param>
        /// <param name="text">Der Text der dargestellt werden soll.</param>
        public void AddNewTextLine(Level l, OutputType t, string text)
        {
            if (t == OutputType.None) return;
            var section = Document.Sections.Find(s => s.Headline == CurrentSection);
            if (section == null)
            {
                section = new OMSection();
                section.Headline = CurrentSection;
                Document.Sections.Add(section);
            }
            var lastBlock = section.Textblocks.LastOrDefault();
            CurrentTextline = null;
            OMTextLine? lastTextline = null;
            OMTextBlock? lastSubblock = null;
            OMTextLine? lastSubtextline = null;
            OMTextBlock? lastSubsubblock = null;
            if (lastBlock != null)
            {
                lastTextline = lastBlock.TextLines.LastOrDefault();
                if (lastTextline != null)
                {
                    lastSubblock = lastTextline.Subblocks.LastOrDefault();
                    if (lastSubblock != null)
                    {
                        lastSubtextline = lastSubblock.TextLines.LastOrDefault();
                        if (lastSubtextline != null)
                        {
                            lastSubsubblock = lastSubtextline.Subblocks.LastOrDefault();
                        }
                    }
                }                
            }
            if (l == Level.Level1)
            {
                if (lastBlock == null || lastBlock.BlockType != t || lastBlock.Closed)
                {
                    lastBlock = new OMTextBlock();
                    lastBlock.ID = "B" + (++IDCounter).ToString();
                    lastBlock.BlockType = t;
                    lastBlock.NumerationType = "1";
                    section.Textblocks.Add(lastBlock);
                }
                CurrentTextline = new OMTextLine();
                lastBlock.TextLines.Add(CurrentTextline);
                CurrentTextBlock = lastBlock;
            }
            else if (l == Level.Level2 && lastBlock != null)
            {                                                 
                if (lastTextline != null && (lastSubblock == null || lastSubblock.BlockType != t))
                {
                    lastSubblock = new OMTextBlock();
                    lastSubblock.ID = "B" + (++IDCounter).ToString();
                    lastSubblock.BlockType = t;
                    lastSubblock.NumerationType = "a";
                    lastTextline.Subblocks.Add(lastSubblock);
                }
                CurrentTextline = new OMTextLine();
                if (lastSubblock != null)
                {
                    lastSubblock.TextLines.Add(CurrentTextline);            
                }
            }
            else if (l == Level.Level3 && lastSubblock != null)
            {
                if (lastSubtextline != null && (lastSubsubblock == null || lastSubsubblock.BlockType != t))
                {
                    lastSubsubblock = new OMTextBlock();
                    lastSubsubblock.ID = "B" + (++IDCounter).ToString();
                    lastSubsubblock.BlockType = t;
                    lastSubsubblock.NumerationType = "i";
                    lastSubtextline.Subblocks.Add(lastSubsubblock);
                }
                CurrentTextline = new OMTextLine();
                if (lastSubsubblock != null)
                {
                    lastSubsubblock.TextLines.Add(CurrentTextline);
                }
            }
            if (CurrentTextline != null)
            {
                AddNewTextElement(text, string.Empty, false, false);
            }
        }

        /// <summary>
        /// Fügt ein Textelement zur letzten Textzeile hinzu.
        /// </summary>
        /// <param name="text">Der Text der dargestellt werden soll.</param>
        /// <param name="link">URL eines Links, falls der Text als Link dargestellt werden soll. string.Empty falls nicht.</param>
        /// <param name="codeformat">True, wenn das Textelement als Code formatiert werden soll.</param>
        public void AddNewTextElement(string text, string link, bool codeformat, bool isonwhitelist)
        {
            if (CurrentTextline != null && !string.IsNullOrEmpty(text))
            {
                OMTextElement newtextelement = new OMTextElement();
                newtextelement.Text = text;
                newtextelement.Link = link;
                newtextelement.IsOnWhitelist = isonwhitelist;
                newtextelement.Codeformat = codeformat;
                CurrentTextline.TextElements.Add(newtextelement);
            }
        }

        public void EndParagraph()
        {
            if (CurrentTextBlock != null)
            {
                CurrentTextBlock.Closed = true;
            }
        }

        public void MoveSection(string fromsection, string tosection)
        {
            var fsec = Document.Sections.Find(s => s.Headline == fromsection);
            if (fsec != null)
            {
                var tsec = Document.Sections.Find(s => s.Headline == tosection);
                if (tsec == null)
                {
                    tsec = new OMSection();
                    tsec.Headline = tosection;
                    Document.Sections.Add(tsec);
                }
                if (fsec.Textblocks.Any() && tsec.Textblocks.Any())
                {
                    var fftb = fsec.Textblocks.First();
                    var tltb = tsec.Textblocks.Last();
                    if (fftb != null && tltb != null && fftb.BlockType == tltb.BlockType)
                    {
                        // Die aufeinandertreffenden Textblöcke haben den gleichen Typ und werden verschmolzen
                        tltb.TextLines.AddRange(fftb.TextLines);
                        fsec.Textblocks.Remove(fftb);
                    }
                }
                tsec.Textblocks.AddRange(fsec.Textblocks);
                Document.Sections.Remove(fsec);
            }
        }
    }
}