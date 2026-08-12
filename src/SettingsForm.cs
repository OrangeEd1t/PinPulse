using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CryptoMonitor
{
    internal sealed class SettingsForm : Form
    {
        private readonly ComboBox languageComboBox;
        private readonly NumericUpDown refreshNumeric;
        private readonly NumericUpDown timeoutNumeric;
        private readonly CheckBox startWithWindowsCheckBox;
        private readonly TextBox displayTemplateTextBox;
        private readonly TextBox itemSeparatorTextBox;
        private readonly NumericUpDown taskbarFixedWidthNumeric;
        private readonly NumericUpDown taskbarMinWidthNumeric;
        private readonly NumericUpDown taskbarMaxWidthNumeric;
        private readonly ComboBox taskbarFontFamilyComboBox;
        private readonly NumericUpDown taskbarFontSizeNumeric;
        private readonly CheckBox taskbarFontBoldCheckBox;
        private readonly TextBox windowBackgroundColorTextBox;
        private readonly Panel windowBackgroundPreviewPanel;
        private readonly CheckBox windowBackgroundTransparentCheckBox;

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
        private readonly Button testItemButton;
        private readonly TextBox itemPreviewTextBox;
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
            ShowIcon = false;
            MaximizeBox = false;
            MinimizeBox = false;
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(920, 650);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            TabControl tabs = new TabControl();
            tabs.Left = 12;
            tabs.Top = 12;
            tabs.Width = 896;
            tabs.Height = 560;
            Controls.Add(tabs);

            TabPage generalTab = new TabPage(Localization.Text(config, "GeneralSettings"));
            TabPage windowTab = new TabPage(Localization.Text(config, "TaskbarSettings"));
            TabPage itemsTab = new TabPage(Localization.Text(config, "MonitorItems"));
            tabs.TabPages.Add(generalTab);
            tabs.TabPages.Add(windowTab);
            tabs.TabPages.Add(itemsTab);

            GroupBox basicGroup = AddGroup(generalTab, Localization.Text(config, "BasicSettings"), 14, 14, 710, 150);
            languageComboBox = AddLanguageComboBox(basicGroup, 180, 28, config.Language);
            AddLabel(basicGroup, Localization.Text(config, "Language"), 14, 31, 150);
            refreshNumeric = AddNumeric(basicGroup, 180, 67, 30, 86400, config.RefreshSeconds);
            AddUnitLabel(basicGroup, Localization.Text(config, "UnitSeconds"), refreshNumeric);
            AddLabel(basicGroup, Localization.Text(config, "RefreshSeconds"), 14, 70, 150);
            timeoutNumeric = AddNumeric(basicGroup, 180, 106, 3, 120, config.RequestTimeoutSeconds);
            AddUnitLabel(basicGroup, Localization.Text(config, "UnitSeconds"), timeoutNumeric);
            AddLabel(basicGroup, Localization.Text(config, "TimeoutSeconds"), 14, 109, 150);
            startWithWindowsCheckBox = new CheckBox();
            startWithWindowsCheckBox.Left = 380;
            startWithWindowsCheckBox.Top = 31;
            startWithWindowsCheckBox.Width = 260;
            startWithWindowsCheckBox.Text = Localization.Text(config, "StartWithWindows");
            startWithWindowsCheckBox.Checked = config.StartWithWindows;
            basicGroup.Controls.Add(startWithWindowsCheckBox);

            GroupBox displayGroup = AddGroup(generalTab, Localization.Text(config, "DisplaySettings"), 14, 178, 710, 178);
            displayTemplateTextBox = AddTextBox(displayGroup, 180, 30, 460, config.DisplayTemplate);
            AddLabel(displayGroup, Localization.Text(config, "DisplayTemplate"), 14, 33, 150);
            AddHint(displayGroup, Localization.Text(config, "DisplayTemplateHint"), 180, 58, 480);
            itemSeparatorTextBox = AddTextBox(displayGroup, 180, 103, 260, config.ItemSeparator);
            AddLabel(displayGroup, Localization.Text(config, "ItemSeparator"), 14, 106, 150);
            AddHint(displayGroup, Localization.Text(config, "ItemSeparatorHint"), 180, 131, 480);

            GroupBox widthGroup = AddGroup(windowTab, Localization.Text(config, "TaskbarWidth"), 14, 14, 710, 180);
            taskbarFixedWidthNumeric = AddNumeric(widthGroup, 180, 30, 0, 4000, config.TaskbarFixedWidth);
            AddUnitLabel(widthGroup, Localization.Text(config, "UnitPixels"), taskbarFixedWidthNumeric);
            AddLabel(widthGroup, Localization.Text(config, "TaskbarFixedWidth"), 14, 33, 150);
            taskbarMinWidthNumeric = AddNumeric(widthGroup, 180, 72, 80, 4000, config.TaskbarMinWidth);
            AddUnitLabel(widthGroup, Localization.Text(config, "UnitPixels"), taskbarMinWidthNumeric);
            AddLabel(widthGroup, Localization.Text(config, "TaskbarMinWidth"), 14, 75, 150);
            taskbarMaxWidthNumeric = AddNumeric(widthGroup, 180, 114, 80, 4000, Math.Max(config.TaskbarMaxWidth, config.TaskbarMinWidth));
            AddUnitLabel(widthGroup, Localization.Text(config, "UnitPixels"), taskbarMaxWidthNumeric);
            AddLabel(widthGroup, Localization.Text(config, "TaskbarMaxWidth"), 14, 117, 150);
            AddHint(widthGroup, Localization.Text(config, "TaskbarWidthHint"), 180, 146, 480);

            GroupBox appearanceGroup = AddGroup(windowTab, Localization.Text(config, "TaskbarAppearance"), 14, 214, 710, 116);
            taskbarFontFamilyComboBox = AddFontFamilyComboBox(appearanceGroup, 180, 30, config.TaskbarFontFamily);
            AddLabel(appearanceGroup, Localization.Text(config, "TaskbarFontFamily"), 14, 33, 150);
            taskbarFontSizeNumeric = AddNumeric(appearanceGroup, 180, 72, 6, 36, config.TaskbarFontSize);
            AddUnitLabel(appearanceGroup, Localization.Text(config, "UnitPoints"), taskbarFontSizeNumeric);
            AddLabel(appearanceGroup, Localization.Text(config, "TaskbarFontSize"), 14, 75, 150);
            taskbarFontBoldCheckBox = new CheckBox();
            taskbarFontBoldCheckBox.Left = 380;
            taskbarFontBoldCheckBox.Top = 74;
            taskbarFontBoldCheckBox.Width = 120;
            taskbarFontBoldCheckBox.Text = Localization.Text(config, "TaskbarFontBold");
            taskbarFontBoldCheckBox.Checked = config.TaskbarFontBold;
            appearanceGroup.Controls.Add(taskbarFontBoldCheckBox);

            GroupBox backgroundGroup = AddGroup(windowTab, Localization.Text(config, "WindowBackground"), 14, 350, 710, 96);
            windowBackgroundColorTextBox = AddTextBox(backgroundGroup, 180, 30, 110, AppConfig.NormalizeColorHex(config.WindowBackgroundColor, "#FFFFFF"));
            windowBackgroundColorTextBox.TextChanged += WindowBackgroundColorTextBoxChanged;
            AddLabel(backgroundGroup, Localization.Text(config, "WindowBackgroundColor"), 14, 33, 150);
            windowBackgroundPreviewPanel = new Panel();
            windowBackgroundPreviewPanel.Left = 300;
            windowBackgroundPreviewPanel.Top = 30;
            windowBackgroundPreviewPanel.Width = 28;
            windowBackgroundPreviewPanel.Height = 22;
            windowBackgroundPreviewPanel.BorderStyle = BorderStyle.FixedSingle;
            backgroundGroup.Controls.Add(windowBackgroundPreviewPanel);
            Button chooseBackgroundColorButton = AddButton(backgroundGroup, Localization.Text(config, "ChooseColor"), 338, 27, 72, ChooseBackgroundColorButtonClick);
            windowBackgroundTransparentCheckBox = new CheckBox();
            windowBackgroundTransparentCheckBox.Left = 180;
            windowBackgroundTransparentCheckBox.Top = 64;
            windowBackgroundTransparentCheckBox.Width = 220;
            windowBackgroundTransparentCheckBox.Text = Localization.Text(config, "WindowBackgroundTransparent");
            windowBackgroundTransparentCheckBox.Checked = config.WindowBackgroundTransparent;
            backgroundGroup.Controls.Add(windowBackgroundTransparentCheckBox);
            UpdateBackgroundPreview();

            itemListBox = new ListBox();
            itemListBox.Left = 14;
            itemListBox.Top = 58;
            itemListBox.Width = 250;
            itemListBox.Height = 360;
            itemListBox.SelectedIndexChanged += ItemListBoxSelectedIndexChanged;
            itemsTab.Controls.Add(itemListBox);

            presetComboBox = new ComboBox();
            presetComboBox.Left = 14;
            presetComboBox.Top = 18;
            presetComboBox.Width = 156;
            presetComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            AddPresetItems();
            itemsTab.Controls.Add(presetComboBox);

            Button addPresetButton = AddButton(itemsTab, Localization.Text(config, "AddPreset"), 178, 17, 86, AddPresetButtonClick);
            Button addCustomButton = AddButton(itemsTab, Localization.Text(config, "AddCustom"), 14, 430, 118, AddCustomButtonClick);
            Button duplicateButton = AddButton(itemsTab, Localization.Text(config, "Duplicate"), 146, 430, 118, DuplicateButtonClick);
            Button deleteButton = AddButton(itemsTab, Localization.Text(config, "Delete"), 14, 468, 118, DeleteButtonClick);
            Button upButton = AddButton(itemsTab, Localization.Text(config, "MoveUp"), 146, 468, 56, MoveUpButtonClick);
            Button downButton = AddButton(itemsTab, Localization.Text(config, "MoveDown"), 208, 468, 56, MoveDownButtonClick);

            itemEditorPanel = new Panel();
            itemEditorPanel.Left = 284;
            itemEditorPanel.Top = 18;
            itemEditorPanel.Width = 580;
            itemEditorPanel.Height = 500;
            itemEditorPanel.BorderStyle = BorderStyle.FixedSingle;
            itemsTab.Controls.Add(itemEditorPanel);

            itemEnabledCheckBox = new CheckBox();
            itemEnabledCheckBox.Left = 14;
            itemEnabledCheckBox.Top = 14;
            itemEnabledCheckBox.Width = 180;
            itemEnabledCheckBox.Text = Localization.Text(config, "ItemEnabled");
            itemEditorPanel.Controls.Add(itemEnabledCheckBox);

            itemNameTextBox = AddTextBox(itemEditorPanel, 130, 50, 390, "");
            AddLabel(itemEditorPanel, Localization.Text(config, "ItemName"), 14, 53, 105);
            itemTemplateTextBox = AddTextBox(itemEditorPanel, 130, 88, 390, "");
            AddLabel(itemEditorPanel, Localization.Text(config, "ItemTemplate"), 14, 91, 105);
            AddHint(itemEditorPanel, Localization.Text(config, "ItemTemplateHint"), 130, 116, 410);
            itemUrlTextBox = AddTextBox(itemEditorPanel, 130, 150, 410, "");
            AddLabel(itemEditorPanel, Localization.Text(config, "ItemUrl"), 14, 153, 105);
            itemIntervalNumeric = AddNumeric(itemEditorPanel, 130, 188, 30, 86400, config.RefreshSeconds);
            AddUnitLabel(itemEditorPanel, Localization.Text(config, "UnitSeconds"), itemIntervalNumeric);
            AddLabel(itemEditorPanel, Localization.Text(config, "ItemInterval"), 14, 191, 105);
            itemTimeoutNumeric = AddNumeric(itemEditorPanel, 130, 226, 3, 120, config.RequestTimeoutSeconds);
            AddUnitLabel(itemEditorPanel, Localization.Text(config, "UnitSeconds"), itemTimeoutNumeric);
            AddLabel(itemEditorPanel, Localization.Text(config, "ItemTimeout"), 14, 229, 105);

            GroupBox advancedGroup = AddGroup(itemEditorPanel, Localization.Text(config, "AdvancedSettings"), 14, 270, 526, 116);
            itemIdTextBox = AddTextBox(advancedGroup, 116, 26, 150, "");
            AddLabel(advancedGroup, Localization.Text(config, "ItemId"), 12, 29, 90);
            itemMethodComboBox = AddMethodComboBox(advancedGroup, 376, 24);
            AddLabel(advancedGroup, Localization.Text(config, "ItemMethod"), 306, 29, 62);
            itemHeadersTextBox = AddMultilineTextBox(advancedGroup, 116, 62, 150, 40, "");
            AddLabel(advancedGroup, Localization.Text(config, "ItemHeaders"), 12, 65, 90);
            itemBodyTextBox = AddMultilineTextBox(advancedGroup, 376, 62, 136, 40, "");
            AddLabel(advancedGroup, Localization.Text(config, "ItemBody"), 306, 65, 62);

            testItemButton = AddButton(itemEditorPanel, Localization.Text(config, "TestItem"), 14, 404, 96, TestItemButtonClick);
            itemPreviewTextBox = AddMultilineTextBox(itemEditorPanel, 130, 404, 410, 76, "");
            itemPreviewTextBox.ReadOnly = true;

            ReloadItemList(editingItems.Count > 0 ? 0 : -1);
            SetEditorEnabled(editingItems.Count > 0);

            Button saveButton = new Button();
            saveButton.Text = Localization.Text(config, "Save");
            saveButton.Left = 736;
            saveButton.Top = 596;
            saveButton.Width = 80;
            saveButton.DialogResult = DialogResult.OK;
            saveButton.Click += SaveButtonClick;
            Controls.Add(saveButton);

            Button cancelButton = new Button();
            cancelButton.Text = Localization.Text(config, "Cancel");
            cancelButton.Left = 824;
            cancelButton.Top = 596;
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

            ApplyDpiLayoutScale();
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

            try
            {
                StartupManager.SetEnabled(startWithWindowsCheckBox.Checked);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, Localization.Text(Config, "StartupUpdateFailed") + Environment.NewLine + ex.Message, "CryptoMonitor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

            Config.Language = GetSelectedLanguage();
            Config.RefreshSeconds = Convert.ToInt32(refreshNumeric.Value);
            Config.RequestTimeoutSeconds = Convert.ToInt32(timeoutNumeric.Value);
            Config.DisplayTemplate = String.IsNullOrWhiteSpace(displayTemplateTextBox.Text) ? "{items}" : displayTemplateTextBox.Text;
            Config.ItemSeparator = itemSeparatorTextBox.Text;
            Config.StartWithWindows = startWithWindowsCheckBox.Checked;
            Config.TaskbarFixedWidth = Convert.ToInt32(taskbarFixedWidthNumeric.Value);
            Config.TaskbarMinWidth = Convert.ToInt32(taskbarMinWidthNumeric.Value);
            Config.TaskbarMaxWidth = Math.Max(Convert.ToInt32(taskbarMaxWidthNumeric.Value), Config.TaskbarMinWidth);
            Config.TaskbarFontFamily = GetSelectedFontFamily();
            Config.TaskbarFontSize = Convert.ToInt32(taskbarFontSizeNumeric.Value);
            Config.TaskbarFontBold = taskbarFontBoldCheckBox.Checked;
            Config.WindowBackgroundColor = GetSelectedBackgroundColor();
            Config.WindowBackgroundTransparent = windowBackgroundTransparentCheckBox.Checked;
            Config.Items = CloneItems(editingItems);
        }

        private void ChooseBackgroundColorButtonClick(object sender, EventArgs e)
        {
            using (ColorDialog dialog = new ColorDialog())
            {
                Color color;
                dialog.Color = TryParseColorHex(windowBackgroundColorTextBox.Text, out color) ? color : Color.White;
                dialog.FullOpen = true;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    windowBackgroundColorTextBox.Text = ColorToHex(dialog.Color);
                }
            }
        }

        private void WindowBackgroundColorTextBoxChanged(object sender, EventArgs e)
        {
            UpdateBackgroundPreview();
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
            ApiItemPresetOption option = presetComboBox.SelectedItem as ApiItemPresetOption;
            ApiItemConfig item = option == null
                ? CreateBlankItem()
                : option.Create(Convert.ToInt32(refreshNumeric.Value), Convert.ToInt32(timeoutNumeric.Value));
            item.Id = UniqueItemId(item.Id);
            editingItems.Add(item);
            ReloadItemList(editingItems.Count - 1);
        }

        private void AddCustomButtonClick(object sender, EventArgs e)
        {
            SaveCurrentEditor(false);
            ApiItemConfig item = CreateBlankItem();
            item.Id = UniqueItemId(item.Id);
            editingItems.Add(item);
            ReloadItemList(editingItems.Count - 1);
        }

        private void TestItemButtonClick(object sender, EventArgs e)
        {
            if (!SaveCurrentEditor(true) || currentItemIndex < 0 || currentItemIndex >= editingItems.Count)
            {
                return;
            }

            ApiItemConfig item = CloneItem(editingItems[currentItemIndex]);
            itemPreviewTextBox.Text = Localization.Text(Config, "TestingItem");
            testItemButton.Enabled = false;

            Task<string> task = Task.Factory.StartNew<string>(delegate
            {
                string response = CryptoPriceService.Download(item, Convert.ToInt32(timeoutNumeric.Value));
                string rendered = JsonTemplateRenderer.Render(response, item.Template);
                return Localization.Text(Config, "PreviewRendered") + Environment.NewLine +
                    rendered + Environment.NewLine + Environment.NewLine +
                    Localization.Text(Config, "PreviewResponse") + Environment.NewLine +
                    Shorten(response, 3000);
            });

            task.ContinueWith(delegate(Task<string> completed)
            {
                if (IsDisposed)
                {
                    return;
                }

                try
                {
                    BeginInvoke(new MethodInvoker(delegate
                    {
                        testItemButton.Enabled = currentItemIndex >= 0;
                        if (completed.IsFaulted)
                        {
                            Exception ex = completed.Exception == null ? null : completed.Exception.GetBaseException();
                            itemPreviewTextBox.Text = Localization.Text(Config, "TestItemFailed") + Environment.NewLine + (ex == null ? "" : ex.Message);
                        }
                        else
                        {
                            itemPreviewTextBox.Text = completed.Result;
                        }
                    }));
                }
                catch (InvalidOperationException)
                {
                    // The form may have closed while the request was still running.
                }
            });
        }

        private void DuplicateButtonClick(object sender, EventArgs e)
        {
            if (currentItemIndex < 0 || currentItemIndex >= editingItems.Count)
            {
                return;
            }

            SaveCurrentEditor(false);
            ApiItemConfig item = CloneItem(editingItems[currentItemIndex]);
            item.Id = UniqueItemId(item.Id + "-copy");
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
            itemPreviewTextBox.Text = "";
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
                itemListBox.Items.Add(new ApiItemListOption(editingItems[i], i, Localization.Text(Config, "UnitSeconds")));
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

            int selectedIndex = itemListBox.SelectedIndex;
            loadingItem = true;
            try
            {
                itemListBox.Items[currentItemIndex] = new ApiItemListOption(editingItems[currentItemIndex], currentItemIndex, Localization.Text(Config, "UnitSeconds"));
                if (selectedIndex >= 0 && selectedIndex < itemListBox.Items.Count)
                {
                    itemListBox.SelectedIndex = selectedIndex;
                }
            }
            finally
            {
                loadingItem = false;
            }
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
            presetComboBox.Items.Add(new ApiItemPresetOption(Localization.Text(Config, "PresetGetJson"), CreateGetJsonPreset));
            presetComboBox.Items.Add(new ApiItemPresetOption(Localization.Text(Config, "PresetPostJson"), CreatePostJsonPreset));
            presetComboBox.Items.Add(new ApiItemPresetOption(Localization.Text(Config, "PresetFearGreed"), CreateFearGreedPreset));
            presetComboBox.Items.Add(new ApiItemPresetOption(Localization.Text(Config, "PresetCryptoPrice"), CreateCryptoPricePreset));

            if (presetComboBox.Items.Count > 0)
            {
                presetComboBox.SelectedIndex = 0;
            }
        }

        private ApiItemConfig CreateBlankItem()
        {
            ApiItemConfig item = new ApiItemConfig();
            item.Id = "item" + (editingItems.Count + 1).ToString();
            item.Name = Localization.Text(Config, "NewItemName");
            item.Method = "GET";
            item.Template = "${$.value}";
            item.IntervalSeconds = Convert.ToInt32(refreshNumeric.Value);
            item.TimeoutSeconds = Convert.ToInt32(timeoutNumeric.Value);
            item.Headers["Accept"] = "application/json";
            return item;
        }

        private ApiItemConfig CreateGetJsonPreset(int intervalSeconds, int timeoutSeconds)
        {
            ApiItemConfig item = CreateBlankItem();
            item.Id = "get-json";
            item.Name = Localization.Text(Config, "PresetGetJson");
            item.Template = "Value: ${$.value}";
            item.IntervalSeconds = intervalSeconds;
            item.TimeoutSeconds = timeoutSeconds;
            return item;
        }

        private ApiItemConfig CreatePostJsonPreset(int intervalSeconds, int timeoutSeconds)
        {
            ApiItemConfig item = CreateBlankItem();
            item.Id = "post-json";
            item.Name = Localization.Text(Config, "PresetPostJson");
            item.Method = "POST";
            item.Headers["Content-Type"] = "application/json";
            item.Body = "{\r\n  \"key\": \"value\"\r\n}";
            item.Template = "Result: ${$.result}";
            item.IntervalSeconds = intervalSeconds;
            item.TimeoutSeconds = timeoutSeconds;
            return item;
        }

        private ApiItemConfig CreateFearGreedPreset(int intervalSeconds, int timeoutSeconds)
        {
            ApiItemConfig item = CreateBlankItem();
            item.Id = "fear-greed";
            item.Name = "FGI";
            item.Url = "https://api.alternative.me/fng/";
            item.Template = "FGI: ${$.data[0].value} ${$.data[0].value_classification}";
            item.IntervalSeconds = intervalSeconds;
            item.TimeoutSeconds = timeoutSeconds;
            return item;
        }

        private ApiItemConfig CreateCryptoPricePreset(int intervalSeconds, int timeoutSeconds)
        {
            return AppConfig.CreateKnownCoinItem("BTC", intervalSeconds, timeoutSeconds);
        }

        private string UniqueItemId(string preferredId)
        {
            string baseId = String.IsNullOrWhiteSpace(preferredId) ? "item" + (editingItems.Count + 1).ToString() : preferredId.Trim();
            string candidate = baseId;
            int suffix = 2;
            while (ContainsItemId(candidate))
            {
                candidate = baseId + "-" + suffix.ToString();
                suffix++;
            }

            return candidate;
        }

        private bool ContainsItemId(string id)
        {
            for (int i = 0; i < editingItems.Count; i++)
            {
                ApiItemConfig item = editingItems[i];
                if (item != null && String.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
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

        private Label AddUnitLabel(Control parent, string text, Control input)
        {
            Label label = AddLabel(parent, text, input.Right + 8, input.Top + 1, 58);
            label.ForeColor = SystemColors.GrayText;
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
            ComboBox comboBox = new WheelSafeComboBox();
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

        private ComboBox AddMethodComboBox(Control parent, int left, int top)
        {
            ComboBox comboBox = new WheelSafeComboBox();
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

        private ComboBox AddFontFamilyComboBox(Control parent, int left, int top, string fontFamily)
        {
            ComboBox comboBox = new WheelSafeComboBox();
            comboBox.Left = left;
            comboBox.Top = top;
            comboBox.Width = 190;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.MaxDropDownItems = 12;

            using (InstalledFontCollection fonts = new InstalledFontCollection())
            {
                for (int i = 0; i < fonts.Families.Length; i++)
                {
                    comboBox.Items.Add(fonts.Families[i].Name);
                }
            }

            string selectedFont = String.IsNullOrWhiteSpace(fontFamily) ? "Microsoft YaHei UI" : fontFamily;
            int selectedIndex = comboBox.FindStringExact(selectedFont);
            if (selectedIndex < 0)
            {
                comboBox.Items.Insert(0, selectedFont);
                selectedIndex = 0;
            }

            comboBox.SelectedIndex = selectedIndex;
            parent.Controls.Add(comboBox);
            return comboBox;
        }

        private string GetSelectedLanguage()
        {
            CodeOption option = languageComboBox.SelectedItem as CodeOption;
            return option == null ? Localization.English : option.Code;
        }

        private string GetSelectedFontFamily()
        {
            string value = taskbarFontFamilyComboBox.Text.Trim();
            return value.Length == 0 ? "Microsoft YaHei UI" : value;
        }

        private string GetSelectedBackgroundColor()
        {
            return AppConfig.NormalizeColorHex(windowBackgroundColorTextBox.Text, "#FFFFFF");
        }

        private void UpdateBackgroundPreview()
        {
            Color color;
            windowBackgroundPreviewPanel.BackColor = TryParseColorHex(windowBackgroundColorTextBox.Text, out color) ? color : SystemColors.Control;
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

        private static string Shorten(string text, int maxLength)
        {
            if (String.IsNullOrEmpty(text) || text.Length <= maxLength)
            {
                return text;
            }

            return text.Substring(0, maxLength) + Environment.NewLine + "...";
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

        private void ApplyDpiLayoutScale()
        {
            float scale = GetDpiScale();
            if (scale <= 1.05F)
            {
                return;
            }

            SuspendLayout();
            try
            {
                Scale(new SizeF(scale, scale));
                ClientSize = new Size(ScaleValue(920, scale), ScaleValue(650, scale));
                MinimumSize = Size;
            }
            finally
            {
                ResumeLayout(false);
            }
        }

        private static int ScaleValue(int value, float scale)
        {
            return (int)Math.Round(value * scale);
        }

        private static float GetDpiScale()
        {
            IntPtr dc = GetDC(IntPtr.Zero);
            if (dc == IntPtr.Zero)
            {
                return 1F;
            }

            try
            {
                int dpi = GetDeviceCaps(dc, LOGPIXELSX);
                if (dpi <= 0)
                {
                    return 1F;
                }

                return Math.Max(1F, Math.Min(2.5F, dpi / 96F));
            }
            finally
            {
                ReleaseDC(IntPtr.Zero, dc);
            }
        }

        private static bool TryParseColorHex(string value, out Color color)
        {
            color = Color.Empty;
            string normalized = AppConfig.NormalizeColorHex(value, "");
            if (normalized.Length == 0)
            {
                return false;
            }

            try
            {
                color = ColorTranslator.FromHtml(normalized);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string ColorToHex(Color color)
        {
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
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
            private readonly string secondsUnit;

            public ApiItemListOption(ApiItemConfig item, int index, string secondsUnit)
            {
                this.item = item;
                this.index = index;
                this.secondsUnit = secondsUnit;
            }

            public override string ToString()
            {
                string name = item == null ? "" : item.DisplayName(index);
                string mark = item != null && item.Enabled ? "[x] " : "[ ] ";
                if (item == null)
                {
                    return mark + name;
                }

                return mark + name + " (" + item.IntervalSeconds.ToString() + " " + secondsUnit + " / " +
                    item.TimeoutSeconds.ToString() + " " + secondsUnit + ")";
            }
        }

        private sealed class ApiItemPresetOption
        {
            private readonly string name;
            private readonly Func<int, int, ApiItemConfig> factory;

            public ApiItemPresetOption(string name, Func<int, int, ApiItemConfig> factory)
            {
                this.name = name;
                this.factory = factory;
            }

            public ApiItemConfig Create(int intervalSeconds, int timeoutSeconds)
            {
                return factory(intervalSeconds, timeoutSeconds);
            }

            public override string ToString()
            {
                return name;
            }
        }

        private sealed class WheelSafeComboBox : ComboBox
        {
            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_MOUSEWHEEL && DroppedDown && Items.Count > 0)
                {
                    int delta = (short)((m.WParam.ToInt64() >> 16) & 0xffff);
                    int lines = SystemInformation.MouseWheelScrollLines;
                    int step = lines <= 0 ? 1 : lines;
                    int topIndex = SendMessage(Handle, CB_GETTOPINDEX, IntPtr.Zero, IntPtr.Zero).ToInt32();
                    int nextIndex = delta > 0 ? topIndex - step : topIndex + step;
                    nextIndex = Clamp(nextIndex, 0, Items.Count - 1);
                    SendMessage(Handle, CB_SETTOPINDEX, new IntPtr(nextIndex), IntPtr.Zero);
                    m.Result = IntPtr.Zero;
                    return;
                }

                base.WndProc(ref m);
            }

            private const int WM_MOUSEWHEEL = 0x020A;
            private const int CB_GETTOPINDEX = 0x015B;
            private const int CB_SETTOPINDEX = 0x015C;

            [DllImport("user32.dll")]
            private static extern IntPtr SendMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);
        }

        private const int LOGPIXELSX = 88;

        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int index);
    }
}
