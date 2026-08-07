using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CryptoMonitor
{
    internal sealed class SettingsForm : Form
    {
        private readonly ComboBox languageComboBox;
        private readonly NumericUpDown refreshNumeric;
        private readonly NumericUpDown timeoutNumeric;
        private readonly TextBox displayTemplateTextBox;
        private readonly TextBox itemSeparatorTextBox;
        private readonly ComboBox taskbarAnchorComboBox;
        private readonly NumericUpDown taskbarOffsetXNumeric;
        private readonly NumericUpDown taskbarOffsetYNumeric;
        private readonly NumericUpDown taskbarFixedWidthNumeric;
        private readonly NumericUpDown taskbarMinWidthNumeric;
        private readonly NumericUpDown taskbarMaxWidthNumeric;

        private readonly List<ApiItemConfig> editingItems;
        private readonly ListBox itemListBox;
        private readonly ComboBox presetComboBox;
        private readonly CheckBox itemEnabledCheckBox;
        private readonly TextBox itemNameTextBox;
        private readonly TextBox itemIdTextBox;
        private readonly TextBox itemUrlTextBox;
        private readonly ComboBox itemMethodComboBox;
        private readonly TextBox itemTemplateTextBox;
        private readonly NumericUpDown itemIntervalNumeric;
        private readonly NumericUpDown itemTimeoutNumeric;
        private readonly TextBox itemHeadersTextBox;
        private readonly TextBox itemBodyTextBox;
        private readonly Panel itemEditorPanel;

        private int currentItemIndex = -1;
        private bool loadingItem;

        public AppConfig Config;

        public SettingsForm(AppConfig config)
        {
            Config = config;
            editingItems = CloneItems(config.Items);

            Text = Localization.Text(config, "SettingsTitle");
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(780, 570);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            TabControl tabs = new TabControl();
            tabs.Left = 12;
            tabs.Top = 12;
            tabs.Width = 756;
            tabs.Height = 500;
            Controls.Add(tabs);

            TabPage generalTab = new TabPage(Localization.Text(config, "GeneralSettings"));
            TabPage taskbarTab = new TabPage(Localization.Text(config, "TaskbarSettings"));
            TabPage itemsTab = new TabPage(Localization.Text(config, "MonitorItems"));
            tabs.TabPages.Add(generalTab);
            tabs.TabPages.Add(taskbarTab);
            tabs.TabPages.Add(itemsTab);

            GroupBox basicGroup = AddGroup(generalTab, Localization.Text(config, "BasicSettings"), 14, 14, 710, 150);
            languageComboBox = AddLanguageComboBox(basicGroup, 180, 28, config.Language);
            AddLabel(basicGroup, Localization.Text(config, "Language"), 14, 31, 150);
            refreshNumeric = AddNumeric(basicGroup, 180, 67, 30, 86400, config.RefreshSeconds);
            AddLabel(basicGroup, Localization.Text(config, "RefreshSeconds"), 14, 70, 150);
            timeoutNumeric = AddNumeric(basicGroup, 180, 106, 3, 120, config.RequestTimeoutSeconds);
            AddLabel(basicGroup, Localization.Text(config, "TimeoutSeconds"), 14, 109, 150);

            GroupBox displayGroup = AddGroup(generalTab, Localization.Text(config, "DisplaySettings"), 14, 178, 710, 178);
            displayTemplateTextBox = AddTextBox(displayGroup, 180, 30, 460, config.DisplayTemplate);
            AddLabel(displayGroup, Localization.Text(config, "DisplayTemplate"), 14, 33, 150);
            AddHint(displayGroup, Localization.Text(config, "DisplayTemplateHint"), 180, 58, 480);
            itemSeparatorTextBox = AddTextBox(displayGroup, 180, 103, 260, config.ItemSeparator);
            AddLabel(displayGroup, Localization.Text(config, "ItemSeparator"), 14, 106, 150);
            AddHint(displayGroup, Localization.Text(config, "ItemSeparatorHint"), 180, 131, 480);

            GroupBox positionGroup = AddGroup(taskbarTab, Localization.Text(config, "TaskbarPosition"), 14, 14, 710, 180);
            taskbarAnchorComboBox = AddAnchorComboBox(positionGroup, 180, 30, config.TaskbarAnchor);
            AddLabel(positionGroup, Localization.Text(config, "TaskbarAnchor"), 14, 33, 150);
            taskbarOffsetXNumeric = AddNumeric(positionGroup, 180, 72, -4000, 4000, config.TaskbarOffsetX);
            AddLabel(positionGroup, Localization.Text(config, "TaskbarOffsetX"), 14, 75, 150);
            taskbarOffsetYNumeric = AddNumeric(positionGroup, 180, 114, -4000, 4000, config.TaskbarOffsetY);
            AddLabel(positionGroup, Localization.Text(config, "TaskbarOffsetY"), 14, 117, 150);
            AddHint(positionGroup, Localization.Text(config, "TaskbarOffsetHint"), 180, 146, 480);

            GroupBox widthGroup = AddGroup(taskbarTab, Localization.Text(config, "TaskbarWidth"), 14, 214, 710, 180);
            taskbarFixedWidthNumeric = AddNumeric(widthGroup, 180, 30, 0, 4000, config.TaskbarFixedWidth);
            AddLabel(widthGroup, Localization.Text(config, "TaskbarFixedWidth"), 14, 33, 150);
            taskbarMinWidthNumeric = AddNumeric(widthGroup, 180, 72, 80, 4000, config.TaskbarMinWidth);
            AddLabel(widthGroup, Localization.Text(config, "TaskbarMinWidth"), 14, 75, 150);
            taskbarMaxWidthNumeric = AddNumeric(widthGroup, 180, 114, 80, 4000, Math.Max(config.TaskbarMaxWidth, config.TaskbarMinWidth));
            AddLabel(widthGroup, Localization.Text(config, "TaskbarMaxWidth"), 14, 117, 150);
            AddHint(widthGroup, Localization.Text(config, "TaskbarWidthHint"), 180, 146, 480);

            itemListBox = new ListBox();
            itemListBox.Left = 14;
            itemListBox.Top = 58;
            itemListBox.Width = 210;
            itemListBox.Height = 300;
            itemListBox.SelectedIndexChanged += ItemListBoxSelectedIndexChanged;
            itemsTab.Controls.Add(itemListBox);

            presetComboBox = new ComboBox();
            presetComboBox.Left = 14;
            presetComboBox.Top = 18;
            presetComboBox.Width = 124;
            presetComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            AddPresetItems();
            itemsTab.Controls.Add(presetComboBox);

            Button addPresetButton = AddButton(itemsTab, Localization.Text(config, "AddPreset"), 144, 17, 80, AddPresetButtonClick);
            Button addCustomButton = AddButton(itemsTab, Localization.Text(config, "AddCustom"), 14, 370, 100, AddCustomButtonClick);
            Button duplicateButton = AddButton(itemsTab, Localization.Text(config, "Duplicate"), 124, 370, 100, DuplicateButtonClick);
            Button deleteButton = AddButton(itemsTab, Localization.Text(config, "Delete"), 14, 408, 100, DeleteButtonClick);
            Button upButton = AddButton(itemsTab, Localization.Text(config, "MoveUp"), 124, 408, 48, MoveUpButtonClick);
            Button downButton = AddButton(itemsTab, Localization.Text(config, "MoveDown"), 176, 408, 48, MoveDownButtonClick);

            itemEditorPanel = new Panel();
            itemEditorPanel.Left = 244;
            itemEditorPanel.Top = 18;
            itemEditorPanel.Width = 480;
            itemEditorPanel.Height = 430;
            itemEditorPanel.BorderStyle = BorderStyle.FixedSingle;
            itemsTab.Controls.Add(itemEditorPanel);

            itemEnabledCheckBox = new CheckBox();
            itemEnabledCheckBox.Left = 14;
            itemEnabledCheckBox.Top = 14;
            itemEnabledCheckBox.Width = 180;
            itemEnabledCheckBox.Text = Localization.Text(config, "ItemEnabled");
            itemEditorPanel.Controls.Add(itemEnabledCheckBox);

            itemNameTextBox = AddTextBox(itemEditorPanel, 130, 50, 290, "");
            AddLabel(itemEditorPanel, Localization.Text(config, "ItemName"), 14, 53, 105);
            itemTemplateTextBox = AddTextBox(itemEditorPanel, 130, 88, 290, "");
            AddLabel(itemEditorPanel, Localization.Text(config, "ItemTemplate"), 14, 91, 105);
            AddHint(itemEditorPanel, Localization.Text(config, "ItemTemplateHint"), 130, 116, 330);
            itemUrlTextBox = AddTextBox(itemEditorPanel, 130, 150, 320, "");
            AddLabel(itemEditorPanel, Localization.Text(config, "ItemUrl"), 14, 153, 105);
            itemIntervalNumeric = AddNumeric(itemEditorPanel, 130, 188, 30, 86400, config.RefreshSeconds);
            AddLabel(itemEditorPanel, Localization.Text(config, "ItemInterval"), 14, 191, 105);
            itemTimeoutNumeric = AddNumeric(itemEditorPanel, 130, 226, 3, 120, config.RequestTimeoutSeconds);
            AddLabel(itemEditorPanel, Localization.Text(config, "ItemTimeout"), 14, 229, 105);

            GroupBox advancedGroup = AddGroup(itemEditorPanel, Localization.Text(config, "AdvancedSettings"), 14, 270, 440, 146);
            itemIdTextBox = AddTextBox(advancedGroup, 116, 26, 125, "");
            AddLabel(advancedGroup, Localization.Text(config, "ItemId"), 12, 29, 90);
            itemMethodComboBox = AddMethodComboBox(advancedGroup, 316, 24);
            AddLabel(advancedGroup, Localization.Text(config, "ItemMethod"), 256, 29, 58);
            itemHeadersTextBox = AddMultilineTextBox(advancedGroup, 116, 62, 125, 58, "");
            AddLabel(advancedGroup, Localization.Text(config, "ItemHeaders"), 12, 65, 90);
            itemBodyTextBox = AddMultilineTextBox(advancedGroup, 316, 62, 110, 58, "");
            AddLabel(advancedGroup, Localization.Text(config, "ItemBody"), 256, 65, 58);

            ReloadItemList(editingItems.Count > 0 ? 0 : -1);
            SetEditorEnabled(editingItems.Count > 0);

            Button saveButton = new Button();
            saveButton.Text = Localization.Text(config, "Save");
            saveButton.Left = 596;
            saveButton.Top = 526;
            saveButton.Width = 80;
            saveButton.DialogResult = DialogResult.OK;
            saveButton.Click += SaveButtonClick;
            Controls.Add(saveButton);

            Button cancelButton = new Button();
            cancelButton.Text = Localization.Text(config, "Cancel");
            cancelButton.Left = 684;
            cancelButton.Top = 526;
            cancelButton.Width = 80;
            cancelButton.DialogResult = DialogResult.Cancel;
            Controls.Add(cancelButton);

            AcceptButton = saveButton;
            CancelButton = cancelButton;

            GC.KeepAlive(addPresetButton);
            GC.KeepAlive(addCustomButton);
            GC.KeepAlive(duplicateButton);
            GC.KeepAlive(deleteButton);
            GC.KeepAlive(upButton);
            GC.KeepAlive(downButton);
        }

        private void SaveButtonClick(object sender, EventArgs e)
        {
            if (!SaveCurrentEditor(true))
            {
                DialogResult = DialogResult.None;
                return;
            }

            if (!ValidateItems())
            {
                DialogResult = DialogResult.None;
                return;
            }

            if (!HasEnabledItems(editingItems))
            {
                MessageBox.Show(this, Localization.Text(Config, "ValidationApiItemsRequired"), "CryptoMonitor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            Config.Language = GetSelectedLanguage();
            Config.RefreshSeconds = Convert.ToInt32(refreshNumeric.Value);
            Config.RequestTimeoutSeconds = Convert.ToInt32(timeoutNumeric.Value);
            Config.DisplayTemplate = String.IsNullOrWhiteSpace(displayTemplateTextBox.Text) ? "{items}" : displayTemplateTextBox.Text;
            Config.ItemSeparator = itemSeparatorTextBox.Text;
            Config.TaskbarAnchor = GetSelectedAnchor();
            Config.TaskbarOffsetX = Convert.ToInt32(taskbarOffsetXNumeric.Value);
            Config.TaskbarOffsetY = Convert.ToInt32(taskbarOffsetYNumeric.Value);
            Config.TaskbarFixedWidth = Convert.ToInt32(taskbarFixedWidthNumeric.Value);
            Config.TaskbarMinWidth = Convert.ToInt32(taskbarMinWidthNumeric.Value);
            Config.TaskbarMaxWidth = Math.Max(Convert.ToInt32(taskbarMaxWidthNumeric.Value), Config.TaskbarMinWidth);
            Config.Items = CloneItems(editingItems);
            Config.ShowTaskbarWindow = true;
        }

        private void ItemListBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (loadingItem)
            {
                return;
            }

            SaveCurrentEditor(false);
            currentItemIndex = itemListBox.SelectedIndex;
            LoadCurrentEditor();
        }

        private void AddPresetButtonClick(object sender, EventArgs e)
        {
            SaveCurrentEditor(false);
            string symbol = Convert.ToString(presetComboBox.SelectedItem);
            ApiItemConfig item = AppConfig.CreateKnownCoinItem(symbol, Convert.ToInt32(refreshNumeric.Value), Convert.ToInt32(timeoutNumeric.Value));
            editingItems.Add(item);
            ReloadItemList(editingItems.Count - 1);
        }

        private void AddCustomButtonClick(object sender, EventArgs e)
        {
            SaveCurrentEditor(false);
            ApiItemConfig item = new ApiItemConfig();
            item.Id = "custom" + (editingItems.Count + 1).ToString();
            item.Name = Localization.Text(Config, "NewItemName");
            item.IntervalSeconds = Convert.ToInt32(refreshNumeric.Value);
            item.TimeoutSeconds = Convert.ToInt32(timeoutNumeric.Value);
            editingItems.Add(item);
            ReloadItemList(editingItems.Count - 1);
        }

        private void DuplicateButtonClick(object sender, EventArgs e)
        {
            if (currentItemIndex < 0 || currentItemIndex >= editingItems.Count)
            {
                return;
            }

            SaveCurrentEditor(false);
            ApiItemConfig item = CloneItem(editingItems[currentItemIndex]);
            item.Id = item.Id + "-copy";
            item.Name = item.Name + " Copy";
            editingItems.Insert(currentItemIndex + 1, item);
            ReloadItemList(currentItemIndex + 1);
        }

        private void DeleteButtonClick(object sender, EventArgs e)
        {
            if (currentItemIndex < 0 || currentItemIndex >= editingItems.Count)
            {
                return;
            }

            editingItems.RemoveAt(currentItemIndex);
            int nextIndex = editingItems.Count == 0 ? -1 : Math.Min(currentItemIndex, editingItems.Count - 1);
            currentItemIndex = -1;
            ReloadItemList(nextIndex);
        }

        private void MoveUpButtonClick(object sender, EventArgs e)
        {
            MoveCurrentItem(-1);
        }

        private void MoveDownButtonClick(object sender, EventArgs e)
        {
            MoveCurrentItem(1);
        }

        private void MoveCurrentItem(int direction)
        {
            if (currentItemIndex < 0 || currentItemIndex >= editingItems.Count)
            {
                return;
            }

            int target = currentItemIndex + direction;
            if (target < 0 || target >= editingItems.Count)
            {
                return;
            }

            SaveCurrentEditor(false);
            ApiItemConfig item = editingItems[currentItemIndex];
            editingItems.RemoveAt(currentItemIndex);
            editingItems.Insert(target, item);
            ReloadItemList(target);
        }

        private bool SaveCurrentEditor(bool showErrors)
        {
            if (currentItemIndex < 0 || currentItemIndex >= editingItems.Count)
            {
                return true;
            }

            ApiItemConfig item = editingItems[currentItemIndex];
            Dictionary<string, string> headers;
            string headerError;
            if (!TryReadHeaders(itemHeadersTextBox.Text, out headers, out headerError))
            {
                if (showErrors)
                {
                    MessageBox.Show(this, Localization.Text(Config, "ValidationHeadersInvalid") + Environment.NewLine + headerError, "CryptoMonitor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
            else
            {
                item.Headers = headers;
            }

            item.Enabled = itemEnabledCheckBox.Checked;
            item.Name = itemNameTextBox.Text.Trim();
            item.Id = itemIdTextBox.Text.Trim();
            item.Url = itemUrlTextBox.Text.Trim();
            item.Method = Convert.ToString(itemMethodComboBox.SelectedItem);
            item.Template = itemTemplateTextBox.Text.Trim();
            item.IntervalSeconds = Convert.ToInt32(itemIntervalNumeric.Value);
            item.TimeoutSeconds = Convert.ToInt32(itemTimeoutNumeric.Value);
            item.Body = itemBodyTextBox.Text;

            if (showErrors && item.Url.Length == 0)
            {
                MessageBox.Show(this, Localization.Text(Config, "ValidationApiUrlRequired"), "CryptoMonitor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            RefreshCurrentListItem();
            return true;
        }

        private bool ValidateItems()
        {
            for (int i = 0; i < editingItems.Count; i++)
            {
                ApiItemConfig item = editingItems[i];
                if (item != null && item.Enabled && String.IsNullOrWhiteSpace(item.Url))
                {
                    ReloadItemList(i);
                    MessageBox.Show(this, Localization.Text(Config, "ValidationApiUrlRequired"), "CryptoMonitor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            return true;
        }

        private void LoadCurrentEditor()
        {
            if (currentItemIndex < 0 || currentItemIndex >= editingItems.Count)
            {
                SetEditorEnabled(false);
                return;
            }

            loadingItem = true;
            ApiItemConfig item = editingItems[currentItemIndex];
            itemEnabledCheckBox.Checked = item.Enabled;
            itemNameTextBox.Text = item.Name;
            itemIdTextBox.Text = item.Id;
            itemUrlTextBox.Text = item.Url;
            itemTemplateTextBox.Text = item.Template;
            itemIntervalNumeric.Value = Clamp(item.IntervalSeconds, 30, 86400);
            itemTimeoutNumeric.Value = Clamp(item.TimeoutSeconds, 3, 120);
            itemHeadersTextBox.Text = HeadersToText(item.Headers);
            itemBodyTextBox.Text = item.Body;
            SelectMethod(item.Method);
            SetEditorEnabled(true);
            loadingItem = false;
        }

        private void ReloadItemList(int selectedIndex)
        {
            loadingItem = true;
            itemListBox.Items.Clear();
            for (int i = 0; i < editingItems.Count; i++)
            {
                itemListBox.Items.Add(new ApiItemListOption(editingItems[i], i));
            }

            if (selectedIndex >= 0 && selectedIndex < itemListBox.Items.Count)
            {
                itemListBox.SelectedIndex = selectedIndex;
                currentItemIndex = selectedIndex;
            }
            else
            {
                currentItemIndex = -1;
            }

            loadingItem = false;
            LoadCurrentEditor();
        }

        private void RefreshCurrentListItem()
        {
            if (currentItemIndex < 0 || currentItemIndex >= editingItems.Count)
            {
                return;
            }

            itemListBox.Items[currentItemIndex] = new ApiItemListOption(editingItems[currentItemIndex], currentItemIndex);
        }

        private void SetEditorEnabled(bool enabled)
        {
            foreach (Control control in itemEditorPanel.Controls)
            {
                control.Enabled = enabled;
            }
        }

        private void AddPresetItems()
        {
            string[] symbols = new string[] { "BTC", "ETH", "SOL", "XRP", "DOGE", "ADA", "BNB", "TRX", "DOT" };
            for (int i = 0; i < symbols.Length; i++)
            {
                presetComboBox.Items.Add(symbols[i]);
            }

            if (presetComboBox.Items.Count > 0)
            {
                presetComboBox.SelectedIndex = 0;
            }
        }

        private Label AddLabel(Control parent, string text, int left, int top, int width)
        {
            Label label = new Label();
            label.Text = text;
            label.Left = left;
            label.Top = top;
            label.Width = width;
            label.Height = 22;
            label.TextAlign = ContentAlignment.MiddleLeft;
            parent.Controls.Add(label);
            return label;
        }

        private Label AddHint(Control parent, string text, int left, int top, int width)
        {
            Label label = new Label();
            label.Text = text;
            label.Left = left;
            label.Top = top;
            label.Width = width;
            label.Height = 34;
            label.ForeColor = SystemColors.GrayText;
            parent.Controls.Add(label);
            return label;
        }

        private GroupBox AddGroup(Control parent, string text, int left, int top, int width, int height)
        {
            GroupBox group = new GroupBox();
            group.Text = text;
            group.Left = left;
            group.Top = top;
            group.Width = width;
            group.Height = height;
            parent.Controls.Add(group);
            return group;
        }

        private Button AddButton(Control parent, string text, int left, int top, int width, EventHandler handler)
        {
            Button button = new Button();
            button.Text = text;
            button.Left = left;
            button.Top = top;
            button.Width = width;
            button.Height = 28;
            button.Click += handler;
            parent.Controls.Add(button);
            return button;
        }

        private TextBox AddTextBox(Control parent, int left, int top, int width, string text)
        {
            TextBox textBox = new TextBox();
            textBox.Left = left;
            textBox.Top = top;
            textBox.Width = width;
            textBox.Text = text;
            parent.Controls.Add(textBox);
            return textBox;
        }

        private TextBox AddMultilineTextBox(Control parent, int left, int top, int width, int height, string text)
        {
            TextBox textBox = new TextBox();
            textBox.Left = left;
            textBox.Top = top;
            textBox.Width = width;
            textBox.Height = height;
            textBox.Multiline = true;
            textBox.ScrollBars = ScrollBars.Vertical;
            textBox.Text = text;
            parent.Controls.Add(textBox);
            return textBox;
        }

        private ComboBox AddLanguageComboBox(Control parent, int left, int top, string language)
        {
            ComboBox comboBox = new ComboBox();
            comboBox.Left = left;
            comboBox.Top = top;
            comboBox.Width = 140;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.Items.Add(new CodeOption(Localization.LanguageDisplayName(Localization.Chinese), Localization.Chinese));
            comboBox.Items.Add(new CodeOption(Localization.LanguageDisplayName(Localization.English), Localization.English));

            string normalized = Localization.NormalizeLanguage(language);
            SelectCodeOption(comboBox, normalized, 0);
            parent.Controls.Add(comboBox);
            return comboBox;
        }

        private ComboBox AddAnchorComboBox(Control parent, int left, int top, string anchor)
        {
            ComboBox comboBox = new ComboBox();
            comboBox.Left = left;
            comboBox.Top = top;
            comboBox.Width = 140;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.Items.Add(new CodeOption(Localization.Text(Config, "TaskbarAnchorLeft"), "left"));
            comboBox.Items.Add(new CodeOption(Localization.Text(Config, "TaskbarAnchorRight"), "right"));
            SelectCodeOption(comboBox, String.Equals(anchor, "right", StringComparison.OrdinalIgnoreCase) ? "right" : "left", 0);
            parent.Controls.Add(comboBox);
            return comboBox;
        }

        private ComboBox AddMethodComboBox(Control parent, int left, int top)
        {
            ComboBox comboBox = new ComboBox();
            comboBox.Left = left;
            comboBox.Top = top;
            comboBox.Width = 110;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.Items.Add("GET");
            comboBox.Items.Add("POST");
            comboBox.Items.Add("PUT");
            comboBox.Items.Add("PATCH");
            comboBox.Items.Add("DELETE");
            comboBox.Items.Add("HEAD");
            comboBox.SelectedIndex = 0;
            parent.Controls.Add(comboBox);
            return comboBox;
        }

        private string GetSelectedLanguage()
        {
            CodeOption option = languageComboBox.SelectedItem as CodeOption;
            return option == null ? Localization.English : option.Code;
        }

        private string GetSelectedAnchor()
        {
            CodeOption option = taskbarAnchorComboBox.SelectedItem as CodeOption;
            return option == null ? "left" : option.Code;
        }

        private NumericUpDown AddNumeric(Control parent, int left, int top, int min, int max, int value)
        {
            NumericUpDown numeric = new NumericUpDown();
            numeric.Left = left;
            numeric.Top = top;
            numeric.Width = 110;
            numeric.Minimum = min;
            numeric.Maximum = max;
            numeric.Value = Clamp(value, min, max);
            parent.Controls.Add(numeric);
            return numeric;
        }

        private void SelectMethod(string method)
        {
            string value = String.IsNullOrEmpty(method) ? "GET" : method.Trim().ToUpperInvariant();
            for (int i = 0; i < itemMethodComboBox.Items.Count; i++)
            {
                if (String.Equals(Convert.ToString(itemMethodComboBox.Items[i]), value, StringComparison.OrdinalIgnoreCase))
                {
                    itemMethodComboBox.SelectedIndex = i;
                    return;
                }
            }

            itemMethodComboBox.SelectedIndex = 0;
        }

        private static void SelectCodeOption(ComboBox comboBox, string code, int fallback)
        {
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                CodeOption option = comboBox.Items[i] as CodeOption;
                if (option != null && String.Equals(option.Code, code, StringComparison.OrdinalIgnoreCase))
                {
                    comboBox.SelectedIndex = i;
                    return;
                }
            }

            comboBox.SelectedIndex = fallback;
        }

        private static bool TryReadHeaders(string text, out Dictionary<string, string> headers, out string error)
        {
            headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            error = null;
            if (String.IsNullOrWhiteSpace(text))
            {
                return true;
            }

            string[] lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                int colon = line.IndexOf(':');
                if (colon <= 0)
                {
                    error = "Line " + (i + 1).ToString() + ": " + line;
                    return false;
                }

                string key = line.Substring(0, colon).Trim();
                string value = line.Substring(colon + 1).Trim();
                if (key.Length == 0)
                {
                    error = "Line " + (i + 1).ToString() + ": " + line;
                    return false;
                }

                headers[key] = value;
            }

            return true;
        }

        private static string HeadersToText(Dictionary<string, string> headers)
        {
            if (headers == null || headers.Count == 0)
            {
                return "";
            }

            List<string> lines = new List<string>();
            foreach (KeyValuePair<string, string> pair in headers)
            {
                lines.Add(pair.Key + ": " + pair.Value);
            }

            return String.Join(Environment.NewLine, lines.ToArray());
        }

        private static bool HasEnabledItems(List<ApiItemConfig> items)
        {
            if (items == null)
            {
                return false;
            }

            for (int i = 0; i < items.Count; i++)
            {
                ApiItemConfig item = items[i];
                if (item != null && item.Enabled)
                {
                    return true;
                }
            }

            return false;
        }

        private static List<ApiItemConfig> CloneItems(List<ApiItemConfig> items)
        {
            List<ApiItemConfig> clones = new List<ApiItemConfig>();
            if (items == null)
            {
                return clones;
            }

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] != null)
                {
                    clones.Add(CloneItem(items[i]));
                }
            }

            return clones;
        }

        private static ApiItemConfig CloneItem(ApiItemConfig source)
        {
            ApiItemConfig item = new ApiItemConfig();
            item.Id = source.Id;
            item.Name = source.Name;
            item.Enabled = source.Enabled;
            item.Url = source.Url;
            item.Method = source.Method;
            item.Body = source.Body;
            item.Template = source.Template;
            item.IntervalSeconds = source.IntervalSeconds;
            item.TimeoutSeconds = source.TimeoutSeconds;
            item.Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (source.Headers != null)
            {
                foreach (KeyValuePair<string, string> pair in source.Headers)
                {
                    item.Headers[pair.Key] = pair.Value;
                }
            }

            return item;
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }

        private sealed class CodeOption
        {
            public readonly string Name;
            public readonly string Code;

            public CodeOption(string name, string code)
            {
                Name = name;
                Code = code;
            }

            public override string ToString()
            {
                return Name;
            }
        }

        private sealed class ApiItemListOption
        {
            private readonly ApiItemConfig item;
            private readonly int index;

            public ApiItemListOption(ApiItemConfig item, int index)
            {
                this.item = item;
                this.index = index;
            }

            public override string ToString()
            {
                string name = item == null ? "" : item.DisplayName(index);
                string mark = item != null && item.Enabled ? "[x] " : "[ ] ";
                return mark + name;
            }
        }
    }
}
