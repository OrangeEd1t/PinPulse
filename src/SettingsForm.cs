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
        private readonly GroupBox displayGroup;
        private readonly ComboBox displayFormatComboBox;
        private readonly Label displayTemplateLabel;
        private readonly TextBox displayTemplateTextBox;
        private readonly Button insertItemsButton;
        private readonly Button insertTimeButton;
        private readonly Button insertDateButton;
        private readonly Button insertCountButton;
        private readonly Label displayTemplateHintLabel;
        private readonly Label itemSeparatorLabel;
        private readonly TextBox itemSeparatorTextBox;
        private readonly Label itemSeparatorHintLabel;
        private readonly Label displayPreviewLabel;
        private readonly RadioButton taskbarAutoWidthRadioButton;
        private readonly RadioButton taskbarFixedWidthRadioButton;
        private readonly Label taskbarFixedWidthLabel;
        private readonly Label taskbarFixedWidthUnitLabel;
        private readonly Label taskbarMinWidthLabel;
        private readonly Label taskbarMinWidthUnitLabel;
        private readonly Label taskbarMaxWidthLabel;
        private readonly Label taskbarMaxWidthUnitLabel;
        private readonly NumericUpDown taskbarFixedWidthNumeric;
        private readonly NumericUpDown taskbarMinWidthNumeric;
        private readonly NumericUpDown taskbarMaxWidthNumeric;
        private readonly Label taskbarWidthHintLabel;
        private readonly ComboBox taskbarFontFamilyComboBox;
        private readonly NumericUpDown taskbarFontSizeNumeric;
        private readonly CheckBox taskbarFontBoldCheckBox;
        private readonly CheckBox windowTextWrapCheckBox;
        private readonly TextBox windowBackgroundColorTextBox;
        private readonly Panel windowBackgroundPreviewPanel;
        private readonly CheckBox windowBackgroundTransparentCheckBox;

        private readonly List<ApiItemConfig> editingItems;
        private readonly ListBox itemListBox;
        private readonly CheckBox itemEnabledCheckBox;
        private readonly TextBox itemNameTextBox;
        private readonly Label itemPrimaryParamLabel;
        private readonly TextBox itemPrimaryParamTextBox;
        private readonly Label itemQuoteCurrencyLabel;
        private readonly ComboBox itemQuoteCurrencyComboBox;
        private readonly TextBox itemIdTextBox;
        private readonly Label itemUrlLabel;
        private readonly TextBox itemUrlTextBox;
        private readonly ComboBox itemMethodComboBox;
        private readonly Label itemTemplateLabel;
        private readonly TextBox itemTemplateTextBox;
        private readonly Label itemTemplateHintLabel;
        private readonly CheckBox itemUseGlobalTimingCheckBox;
        private readonly NumericUpDown itemIntervalNumeric;
        private readonly NumericUpDown itemTimeoutNumeric;
        private readonly TextBox itemHeadersTextBox;
        private readonly TextBox itemBodyTextBox;
        private readonly Button testItemButton;
        private readonly TextBox itemPreviewTextBox;
        private readonly Panel itemEditorPanel;
        private readonly GroupBox itemSourceGroupBox;
        private readonly GroupBox itemTimingGroupBox;
        private readonly GroupBox itemRequestGroupBox;
        private readonly GroupBox itemPreviewGroupBox;
        private readonly List<InputLabelBinding> inputLabelBindings = new List<InputLabelBinding>();

        private int currentItemIndex = -1;
        private bool loadingItem;
        private bool updatingDisplayTemplate;
        private int displayGroupTopPadding;
        private int displayLabelLeft;
        private int displayInputLeft;
        private int displayLabelWidth;
        private int displayRowGap;
        private int displayHintGap;
        private int displayBlockGap;
        private int displayBottomPadding;

        public AppConfig Config;
        public event EventHandler Saved;

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
            AddInputLabel(basicGroup, Localization.Text(config, "Language"), 14, 150, languageComboBox);
            refreshNumeric = AddNumeric(basicGroup, 180, 67, AppConfig.MinRefreshSeconds, AppConfig.MaxRefreshSeconds, config.RefreshSeconds);
            AddUnitLabel(basicGroup, Localization.Text(config, "UnitSeconds"), refreshNumeric);
            AddInputLabel(basicGroup, Localization.Text(config, "RefreshSeconds"), 14, 150, refreshNumeric);
            timeoutNumeric = AddNumeric(basicGroup, 180, 106, 3, 120, config.RequestTimeoutSeconds);
            AddUnitLabel(basicGroup, Localization.Text(config, "UnitSeconds"), timeoutNumeric);
            AddInputLabel(basicGroup, Localization.Text(config, "TimeoutSeconds"), 14, 150, timeoutNumeric);
            startWithWindowsCheckBox = new CheckBox();
            startWithWindowsCheckBox.Left = 380;
            startWithWindowsCheckBox.Top = 31;
            startWithWindowsCheckBox.Width = 260;
            startWithWindowsCheckBox.Text = Localization.Text(config, "StartWithWindows");
            startWithWindowsCheckBox.Checked = config.StartWithWindows;
            basicGroup.Controls.Add(startWithWindowsCheckBox);

            displayGroup = AddGroup(generalTab, Localization.Text(config, "DisplaySettings"), 14, 178, 710, 258);
            displayFormatComboBox = AddDisplayFormatComboBox(displayGroup, 180, 30, config.DisplayTemplate);
            displayFormatComboBox.SelectedIndexChanged += DisplayFormatComboBoxSelectedIndexChanged;
            AddInputLabel(displayGroup, Localization.Text(config, "DisplayFormat"), 14, 150, displayFormatComboBox);
            displayPreviewLabel = AddLabel(displayGroup, "", 180, 246, 480);
            displayPreviewLabel.Height = 28;
            displayPreviewLabel.TextAlign = ContentAlignment.MiddleLeft;
            displayPreviewLabel.AutoSize = false;
            displayPreviewLabel.ForeColor = SystemColors.GrayText;
            displayTemplateTextBox = AddTextBox(displayGroup, 180, 102, 460, config.DisplayTemplate);
            displayTemplateTextBox.TextChanged += DisplayTemplateTextBoxChanged;
            displayTemplateLabel = AddInputLabel(displayGroup, Localization.Text(config, "DisplayTemplate"), 14, 150, displayTemplateTextBox);
            insertItemsButton = AddButton(displayGroup, Localization.Text(config, "TokenItems"), 180, 130, 72, InsertDisplayItemsButtonClick);
            insertTimeButton = AddButton(displayGroup, Localization.Text(config, "TokenTime"), 258, 130, 72, InsertDisplayTimeButtonClick);
            insertDateButton = AddButton(displayGroup, Localization.Text(config, "TokenDate"), 336, 130, 72, InsertDisplayDateButtonClick);
            insertCountButton = AddButton(displayGroup, Localization.Text(config, "TokenCount"), 414, 130, 72, InsertDisplayCountButtonClick);
            displayTemplateHintLabel = AddHint(displayGroup, Localization.Text(config, "DisplayTemplateHint"), 180, 162, 480);
            displayTemplateHintLabel.AutoSize = false;
            itemSeparatorTextBox = AddTextBox(displayGroup, 180, 204, 260, config.ItemSeparator);
            itemSeparatorTextBox.TextChanged += DisplayTemplateTextBoxChanged;
            itemSeparatorLabel = AddInputLabel(displayGroup, Localization.Text(config, "ItemSeparator"), 14, 150, itemSeparatorTextBox);
            itemSeparatorHintLabel = AddHint(displayGroup, Localization.Text(config, "ItemSeparatorHint"), 180, 230, 480);
            itemSeparatorHintLabel.AutoSize = false;
            CaptureDisplayLayoutBaselines();
            SelectDisplayFormatForTemplate(config.DisplayTemplate);
            UpdateDisplayTemplateUi();

            GroupBox widthGroup = AddGroup(windowTab, Localization.Text(config, "TaskbarWidth"), 14, 14, 710, 210);
            taskbarAutoWidthRadioButton = AddRadioButton(widthGroup, Localization.Text(config, "TaskbarWidthAutoMode"), 14, 30, 160, config.TaskbarFixedWidth <= 0);
            taskbarAutoWidthRadioButton.CheckedChanged += TaskbarWidthModeChanged;
            taskbarMinWidthNumeric = AddNumeric(widthGroup, 180, 62, 80, 4000, config.TaskbarMinWidth);
            taskbarMinWidthNumeric.ValueChanged += TaskbarMinWidthNumericValueChanged;
            taskbarMinWidthUnitLabel = AddUnitLabel(widthGroup, Localization.Text(config, "UnitPixels"), taskbarMinWidthNumeric);
            taskbarMinWidthLabel = AddInputLabel(widthGroup, Localization.Text(config, "TaskbarMinWidth"), 42, 120, taskbarMinWidthNumeric);
            taskbarMaxWidthNumeric = AddNumeric(widthGroup, 180, 102, 80, 4000, Math.Max(config.TaskbarMaxWidth, config.TaskbarMinWidth));
            taskbarMaxWidthUnitLabel = AddUnitLabel(widthGroup, Localization.Text(config, "UnitPixels"), taskbarMaxWidthNumeric);
            taskbarMaxWidthLabel = AddInputLabel(widthGroup, Localization.Text(config, "TaskbarMaxWidth"), 42, 120, taskbarMaxWidthNumeric);
            taskbarFixedWidthRadioButton = AddRadioButton(widthGroup, Localization.Text(config, "TaskbarWidthFixedMode"), 14, 144, 160, config.TaskbarFixedWidth > 0);
            taskbarFixedWidthRadioButton.CheckedChanged += TaskbarWidthModeChanged;
            taskbarFixedWidthNumeric = AddNumeric(widthGroup, 180, 142, 80, 4000, config.TaskbarFixedWidth > 0 ? config.TaskbarFixedWidth : config.TaskbarMinWidth);
            taskbarFixedWidthUnitLabel = AddUnitLabel(widthGroup, Localization.Text(config, "UnitPixels"), taskbarFixedWidthNumeric);
            taskbarFixedWidthLabel = AddInputLabel(widthGroup, Localization.Text(config, "TaskbarFixedWidth"), 42, 120, taskbarFixedWidthNumeric);
            taskbarWidthHintLabel = AddHint(widthGroup, Localization.Text(config, "TaskbarWidthHint"), 180, 176, 480);
            UpdateTaskbarWidthModeUi();

            GroupBox appearanceGroup = AddGroup(windowTab, Localization.Text(config, "TaskbarAppearance"), 14, 244, 710, 116);
            taskbarFontFamilyComboBox = AddFontFamilyComboBox(appearanceGroup, 180, 30, config.TaskbarFontFamily);
            AddInputLabel(appearanceGroup, Localization.Text(config, "TaskbarFontFamily"), 14, 150, taskbarFontFamilyComboBox);
            taskbarFontSizeNumeric = AddNumeric(appearanceGroup, 180, 72, 6, 36, config.TaskbarFontSize);
            AddUnitLabel(appearanceGroup, Localization.Text(config, "UnitPoints"), taskbarFontSizeNumeric);
            AddInputLabel(appearanceGroup, Localization.Text(config, "TaskbarFontSize"), 14, 150, taskbarFontSizeNumeric);
            taskbarFontBoldCheckBox = new CheckBox();
            taskbarFontBoldCheckBox.Left = 380;
            taskbarFontBoldCheckBox.Top = 74;
            taskbarFontBoldCheckBox.Width = 120;
            taskbarFontBoldCheckBox.Text = Localization.Text(config, "TaskbarFontBold");
            taskbarFontBoldCheckBox.Checked = config.TaskbarFontBold;
            appearanceGroup.Controls.Add(taskbarFontBoldCheckBox);
            windowTextWrapCheckBox = new CheckBox();
            windowTextWrapCheckBox.Left = 500;
            windowTextWrapCheckBox.Top = 74;
            windowTextWrapCheckBox.Width = 170;
            windowTextWrapCheckBox.Text = Localization.Text(config, "WindowTextWrap");
            windowTextWrapCheckBox.Checked = config.WindowTextWrap;
            appearanceGroup.Controls.Add(windowTextWrapCheckBox);

            GroupBox backgroundGroup = AddGroup(windowTab, Localization.Text(config, "WindowBackground"), 14, 380, 710, 96);
            windowBackgroundColorTextBox = AddTextBox(backgroundGroup, 180, 30, 110, AppConfig.NormalizeColorHex(config.WindowBackgroundColor, "#FFFFFF"));
            windowBackgroundColorTextBox.TextChanged += WindowBackgroundColorTextBoxChanged;
            AddInputLabel(backgroundGroup, Localization.Text(config, "WindowBackgroundColor"), 14, 150, windowBackgroundColorTextBox);
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

            GroupBox currentItemsGroup = AddGroup(itemsTab, Localization.Text(config, "CurrentItems"), 14, 14, 250, 504);
            itemListBox = new ListBox();
            itemListBox.Left = 12;
            itemListBox.Top = 24;
            itemListBox.Width = 222;
            itemListBox.Height = 374;
            itemListBox.SelectedIndexChanged += ItemListBoxSelectedIndexChanged;
            currentItemsGroup.Controls.Add(itemListBox);

            Button addCustomButton = AddButton(currentItemsGroup, Localization.Text(config, "AddCustom"), 12, 410, 104, AddCustomButtonClick);
            Button duplicateButton = AddButton(currentItemsGroup, Localization.Text(config, "Duplicate"), 130, 410, 104, DuplicateButtonClick);
            Button deleteButton = AddButton(currentItemsGroup, Localization.Text(config, "Delete"), 12, 448, 104, DeleteButtonClick);
            Button upButton = AddButton(currentItemsGroup, Localization.Text(config, "MoveUp"), 130, 448, 50, MoveUpButtonClick);
            Button downButton = AddButton(currentItemsGroup, Localization.Text(config, "MoveDown"), 184, 448, 50, MoveDownButtonClick);

            itemEditorPanel = new Panel();
            itemEditorPanel.Left = 284;
            itemEditorPanel.Top = 14;
            itemEditorPanel.Width = 580;
            itemEditorPanel.Height = 504;
            itemEditorPanel.BorderStyle = BorderStyle.FixedSingle;
            itemsTab.Controls.Add(itemEditorPanel);

            GroupBox itemBasicsGroup = AddGroup(itemEditorPanel, Localization.Text(config, "ItemBasics"), 14, 8, 552, 70);
            itemEnabledCheckBox = new CheckBox();
            itemEnabledCheckBox.Left = 12;
            itemEnabledCheckBox.Top = 22;
            itemEnabledCheckBox.Width = 180;
            itemEnabledCheckBox.Text = Localization.Text(config, "ItemEnabled");
            itemEnabledCheckBox.CheckedChanged += ItemPreviewSourceChanged;
            itemBasicsGroup.Controls.Add(itemEnabledCheckBox);

            itemNameTextBox = AddTextBox(itemBasicsGroup, 266, 22, 110, "");
            itemNameTextBox.TextChanged += ItemPreviewSourceChanged;
            AddInputLabel(itemBasicsGroup, Localization.Text(config, "ItemName"), 206, 52, itemNameTextBox);
            itemIdTextBox = AddTextBox(itemBasicsGroup, 422, 22, 106, "");
            itemIdTextBox.TextChanged += ItemPreviewSourceChanged;
            AddInputLabel(itemBasicsGroup, Localization.Text(config, "ItemId"), 390, 24, itemIdTextBox);

            itemSourceGroupBox = AddGroup(itemEditorPanel, Localization.Text(config, "ItemDataSource"), 14, 86, 552, 128);
            itemPrimaryParamTextBox = AddTextBox(itemSourceGroupBox, 116, 28, 190, "");
            itemPrimaryParamTextBox.TextChanged += ItemPreviewSourceChanged;
            itemPrimaryParamLabel = AddInputLabel(itemSourceGroupBox, Localization.Text(config, "ItemSymbol"), 12, 90, itemPrimaryParamTextBox);
            itemQuoteCurrencyComboBox = AddCurrencyComboBox(itemSourceGroupBox, 376, 26, "USD");
            itemQuoteCurrencyComboBox.TextChanged += ItemPreviewSourceChanged;
            itemQuoteCurrencyComboBox.SelectedIndexChanged += ItemPreviewSourceChanged;
            itemQuoteCurrencyLabel = AddInputLabel(itemSourceGroupBox, Localization.Text(config, "ItemQuoteCurrency"), 318, 50, itemQuoteCurrencyComboBox);
            itemUrlTextBox = AddTextBox(itemSourceGroupBox, 116, 28, 412, "");
            itemUrlLabel = AddInputLabel(itemSourceGroupBox, Localization.Text(config, "ItemUrl"), 12, 90, itemUrlTextBox);
            itemTemplateTextBox = AddTextBox(itemSourceGroupBox, 116, 64, 412, "");
            itemTemplateTextBox.TextChanged += ItemPreviewSourceChanged;
            itemTemplateLabel = AddInputLabel(itemSourceGroupBox, Localization.Text(config, "ItemTemplate"), 12, 90, itemTemplateTextBox);
            itemTemplateHintLabel = AddHint(itemSourceGroupBox, Localization.Text(config, "ItemTemplateHint"), 116, 92, 412);

            itemTimingGroupBox = AddGroup(itemEditorPanel, Localization.Text(config, "ItemRefreshSettings"), 14, 222, 552, 72);
            itemUseGlobalTimingCheckBox = new CheckBox();
            itemUseGlobalTimingCheckBox.Left = 12;
            itemUseGlobalTimingCheckBox.Top = 22;
            itemUseGlobalTimingCheckBox.Width = 320;
            itemUseGlobalTimingCheckBox.Text = Localization.Text(config, "ItemUseGlobalTiming");
            itemUseGlobalTimingCheckBox.CheckedChanged += ItemUseGlobalTimingCheckBoxChanged;
            itemTimingGroupBox.Controls.Add(itemUseGlobalTimingCheckBox);
            itemIntervalNumeric = AddNumeric(itemTimingGroupBox, 116, 44, AppConfig.MinRefreshSeconds, AppConfig.MaxRefreshSeconds, config.RefreshSeconds);
            AddUnitLabel(itemTimingGroupBox, Localization.Text(config, "UnitSeconds"), itemIntervalNumeric);
            AddInputLabel(itemTimingGroupBox, Localization.Text(config, "ItemInterval"), 12, 90, itemIntervalNumeric);
            itemTimeoutNumeric = AddNumeric(itemTimingGroupBox, 376, 44, 3, 120, config.RequestTimeoutSeconds);
            AddUnitLabel(itemTimingGroupBox, Localization.Text(config, "UnitSeconds"), itemTimeoutNumeric);
            AddInputLabel(itemTimingGroupBox, Localization.Text(config, "ItemTimeout"), 318, 50, itemTimeoutNumeric);

            itemRequestGroupBox = AddGroup(itemEditorPanel, Localization.Text(config, "ItemRequestSettings"), 14, 302, 552, 84);
            itemMethodComboBox = AddMethodComboBox(itemRequestGroupBox, 116, 24);
            AddInputLabel(itemRequestGroupBox, Localization.Text(config, "ItemMethod"), 12, 90, itemMethodComboBox);
            itemHeadersTextBox = AddMultilineTextBox(itemRequestGroupBox, 116, 54, 190, 24, "");
            AddMultilineInputLabel(itemRequestGroupBox, Localization.Text(config, "ItemHeaders"), 12, 90, itemHeadersTextBox);
            itemBodyTextBox = AddMultilineTextBox(itemRequestGroupBox, 376, 24, 152, 54, "");
            AddMultilineInputLabel(itemRequestGroupBox, Localization.Text(config, "ItemBody"), 318, 50, itemBodyTextBox);

            itemPreviewGroupBox = AddGroup(itemEditorPanel, Localization.Text(config, "ItemTestResult"), 14, 394, 552, 96);
            testItemButton = AddButton(itemPreviewGroupBox, Localization.Text(config, "TestItem"), 12, 24, 96, TestItemButtonClick);
            itemPreviewTextBox = AddMultilineTextBox(itemPreviewGroupBox, 116, 24, 412, 30, "");
            itemPreviewTextBox.ReadOnly = true;

            ReloadItemList(editingItems.Count > 0 ? 0 : -1);
            SetEditorEnabled(editingItems.Count > 0);

            Button saveButton = new Button();
            saveButton.Text = Localization.Text(config, "Save");
            saveButton.Left = 736;
            saveButton.Top = 596;
            saveButton.Width = 80;
            saveButton.Click += SaveButtonClick;
            Controls.Add(saveButton);

            AcceptButton = saveButton;

            GC.KeepAlive(addCustomButton);
            GC.KeepAlive(duplicateButton);
            GC.KeepAlive(deleteButton);
            GC.KeepAlive(upButton);
            GC.KeepAlive(downButton);
            GC.KeepAlive(insertItemsButton);
            GC.KeepAlive(insertTimeButton);
            GC.KeepAlive(insertDateButton);
            GC.KeepAlive(insertCountButton);

            ApplyDpiLayoutScale();
        }

        private void SaveButtonClick(object sender, EventArgs e)
        {
            string displayTemplate = GetDisplayTemplate();
            if (displayTemplate.IndexOf("{items}", StringComparison.Ordinal) < 0)
            {
                MessageBox.Show(this, Localization.Text(Config, "ValidationDisplayTemplateItemsRequired"), "CryptoMonitor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                DialogResult = DialogResult.None;
                return;
            }

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
            Config.DisplayTemplate = displayTemplate;
            Config.ItemSeparator = itemSeparatorTextBox.Text;
            Config.StartWithWindows = startWithWindowsCheckBox.Checked;
            Config.TaskbarFixedWidth = taskbarFixedWidthRadioButton.Checked ? Convert.ToInt32(taskbarFixedWidthNumeric.Value) : 0;
            Config.TaskbarMinWidth = Convert.ToInt32(taskbarMinWidthNumeric.Value);
            Config.TaskbarMaxWidth = Math.Max(Convert.ToInt32(taskbarMaxWidthNumeric.Value), Config.TaskbarMinWidth);
            Config.TaskbarFontFamily = GetSelectedFontFamily();
            Config.TaskbarFontSize = Convert.ToInt32(taskbarFontSizeNumeric.Value);
            Config.TaskbarFontBold = taskbarFontBoldCheckBox.Checked;
            Config.WindowTextWrap = windowTextWrapCheckBox.Checked;
            Config.WindowBackgroundColor = GetSelectedBackgroundColor();
            Config.WindowBackgroundTransparent = windowBackgroundTransparentCheckBox.Checked;
            Config.Items = CloneItems(editingItems);

            EventHandler saved = Saved;
            if (saved != null)
            {
                saved(this, EventArgs.Empty);
            }
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

        private void TaskbarWidthModeChanged(object sender, EventArgs e)
        {
            UpdateTaskbarWidthModeUi();
        }

        private void TaskbarMinWidthNumericValueChanged(object sender, EventArgs e)
        {
            if (taskbarMaxWidthNumeric.Value < taskbarMinWidthNumeric.Value)
            {
                taskbarMaxWidthNumeric.Value = taskbarMinWidthNumeric.Value;
            }
        }

        private void UpdateTaskbarWidthModeUi()
        {
            bool autoWidth = taskbarAutoWidthRadioButton.Checked;
            taskbarMinWidthLabel.Enabled = autoWidth;
            taskbarMinWidthNumeric.Enabled = autoWidth;
            taskbarMinWidthUnitLabel.Enabled = autoWidth;
            taskbarMaxWidthLabel.Enabled = autoWidth;
            taskbarMaxWidthNumeric.Enabled = autoWidth;
            taskbarMaxWidthUnitLabel.Enabled = autoWidth;

            bool fixedWidth = taskbarFixedWidthRadioButton.Checked;
            taskbarFixedWidthLabel.Enabled = fixedWidth;
            taskbarFixedWidthNumeric.Enabled = fixedWidth;
            taskbarFixedWidthUnitLabel.Enabled = fixedWidth;
            taskbarWidthHintLabel.Text = Localization.Text(Config, autoWidth ? "TaskbarAutoWidthHint" : "TaskbarFixedWidthHint");
        }

        private void DisplayFormatComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            if (updatingDisplayTemplate)
            {
                return;
            }

            DisplayFormatOption option = displayFormatComboBox.SelectedItem as DisplayFormatOption;
            if (option == null)
            {
                return;
            }

            if (option.Template != null)
            {
                updatingDisplayTemplate = true;
                try
                {
                    displayTemplateTextBox.Text = option.Template;
                }
                finally
                {
                    updatingDisplayTemplate = false;
                }
            }

            UpdateDisplayTemplateUi();
        }

        private void DisplayTemplateTextBoxChanged(object sender, EventArgs e)
        {
            if (!updatingDisplayTemplate)
            {
                SelectDisplayFormatForTemplate(displayTemplateTextBox.Text);
            }

            UpdateDisplayTemplateUi();
        }

        private void InsertDisplayItemsButtonClick(object sender, EventArgs e)
        {
            InsertDisplayToken("{items}");
        }

        private void InsertDisplayTimeButtonClick(object sender, EventArgs e)
        {
            InsertDisplayToken("{time}");
        }

        private void InsertDisplayDateButtonClick(object sender, EventArgs e)
        {
            InsertDisplayToken("{date}");
        }

        private void InsertDisplayCountButtonClick(object sender, EventArgs e)
        {
            InsertDisplayToken("{count}");
        }

        private void ItemPreviewSourceChanged(object sender, EventArgs e)
        {
            if (!loadingItem)
            {
                UpdateDisplayTemplateUi();
            }
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

        private void AddCustomButtonClick(object sender, EventArgs e)
        {
            SaveCurrentEditor(false);
            ApiItemConfig item = CreateBlankItem();
            item.Id = UniqueItemId(item.Id);
            editingItems.Add(item);
            ReloadItemList(editingItems.Count - 1);
            UpdateDisplayTemplateUi();
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
                string response;
                string rendered = CryptoPriceService.RenderItem(item, Convert.ToInt32(timeoutNumeric.Value), out response);
                return Localization.Text(Config, "PreviewRendered") + Environment.NewLine +
                    rendered + Environment.NewLine + Environment.NewLine +
                    Localization.Text(Config, "PreviewResponse") + Environment.NewLine +
                    Shorten(response, 3000);
            });

            task.ContinueWith(delegate(Task<string> completed)
            {
                if (IsDisposed || !IsHandleCreated)
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
                catch (Exception ex)
                {
                    Program.LogException(ex, "Item test callback could not reach settings UI");
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
            UpdateDisplayTemplateUi();
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
            UpdateDisplayTemplateUi();
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
            UpdateDisplayTemplateUi();
        }

        private bool SaveCurrentEditor(bool showErrors)
        {
            if (currentItemIndex < 0 || currentItemIndex >= editingItems.Count)
            {
                return true;
            }

            ApiItemConfig item = editingItems[currentItemIndex];
            item.Enabled = itemEnabledCheckBox.Checked;
            item.Name = itemNameTextBox.Text.Trim();
            item.Id = itemIdTextBox.Text.Trim();
            item.Type = ApiItemConfig.TypeCustomApi;
            item.IntervalSeconds = itemUseGlobalTimingCheckBox.Checked ? 0 : Convert.ToInt32(itemIntervalNumeric.Value);
            item.TimeoutSeconds = itemUseGlobalTimingCheckBox.Checked ? 0 : Convert.ToInt32(itemTimeoutNumeric.Value);

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

            item.Url = itemUrlTextBox.Text.Trim();
            item.Method = Convert.ToString(itemMethodComboBox.SelectedItem);
            item.Template = itemTemplateTextBox.Text.Trim();
            item.Body = itemBodyTextBox.Text;

            AppConfig.ApplyItemTypeDefaults(item);

            if (showErrors && item.Url.Length == 0)
            {
                MessageBox.Show(this, Localization.Text(Config, "ValidationApiUrlRequired"), "CryptoMonitor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            RefreshCurrentListItem();
            UpdateDisplayTemplateUi();
            return true;
        }

        private bool ValidateItems()
        {
            for (int i = 0; i < editingItems.Count; i++)
            {
                ApiItemConfig item = editingItems[i];
                if (item != null)
                {
                    AppConfig.ApplyItemTypeDefaults(item);
                }

                if (item != null && item.Enabled && item.Type == ApiItemConfig.TypeCoin && AppConfig.AlternativeMeDataId(item.Symbol).Length == 0)
                {
                    ReloadItemList(i);
                    MessageBox.Show(this, Localization.Text(Config, "ValidationCoinSymbolUnsupported"), "CryptoMonitor", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

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
            AppConfig.ApplyItemTypeDefaults(item);
            itemEnabledCheckBox.Checked = item.Enabled;
            itemNameTextBox.Text = item.Name;
            itemIdTextBox.Text = item.Id;
            itemUrlTextBox.Text = item.Url;
            itemTemplateTextBox.Text = item.Template;
            bool usesGlobalTiming = item.IntervalSeconds <= 0 && item.TimeoutSeconds <= 0;
            itemUseGlobalTimingCheckBox.Checked = usesGlobalTiming;
            itemIntervalNumeric.Value = Clamp(item.IntervalSeconds > 0 ? item.IntervalSeconds : Config.RefreshSeconds, AppConfig.MinRefreshSeconds, AppConfig.MaxRefreshSeconds);
            itemTimeoutNumeric.Value = Clamp(item.TimeoutSeconds > 0 ? item.TimeoutSeconds : Config.RequestTimeoutSeconds, 3, 120);
            itemHeadersTextBox.Text = HeadersToText(item.Headers);
            itemBodyTextBox.Text = item.Body;
            itemPreviewTextBox.Text = "";
            SelectMethod(item.Method);
            SetEditorEnabled(true);
            UpdateTimingControls();
            UpdateItemTypeUi();
            loadingItem = false;
        }

        private void UpdateItemTypeUi()
        {
            itemPrimaryParamLabel.Visible = false;
            itemPrimaryParamTextBox.Visible = false;
            itemQuoteCurrencyLabel.Visible = false;
            itemQuoteCurrencyComboBox.Visible = false;
            itemUrlLabel.Visible = true;
            itemUrlTextBox.Visible = true;
            itemTemplateLabel.Visible = true;
            itemTemplateTextBox.Visible = true;
            itemTemplateHintLabel.Visible = true;
            itemRequestGroupBox.Visible = true;

            LayoutItemEditor(true);
        }

        private void LayoutItemEditor(bool custom)
        {
            float scale = Math.Max(1F, itemEditorPanel.Width / 580F);
            itemSourceGroupBox.Height = ScaleValue(custom ? 128 : 70, scale);

            itemTimingGroupBox.Top = itemSourceGroupBox.Bottom + ScaleValue(8, scale);
            itemRequestGroupBox.Top = itemTimingGroupBox.Bottom + ScaleValue(8, scale);
            itemPreviewGroupBox.Top = custom ? itemRequestGroupBox.Bottom + ScaleValue(8, scale) : itemTimingGroupBox.Bottom + ScaleValue(8, scale);

            int previewHeight = itemEditorPanel.Height - itemPreviewGroupBox.Top - ScaleValue(12, scale);
            itemPreviewGroupBox.Height = Math.Max(ScaleValue(66, scale), previewHeight);
            itemPreviewTextBox.Height = Math.Max(ScaleValue(30, scale), itemPreviewGroupBox.Height - ScaleValue(36, scale));
        }

        private void ReloadItemList(int selectedIndex)
        {
            loadingItem = true;
            itemListBox.Items.Clear();
            for (int i = 0; i < editingItems.Count; i++)
            {
                itemListBox.Items.Add(new ApiItemListOption(editingItems[i], i, Localization.Text(Config, "UnitSeconds"), Localization.Text(Config, "GeneralSettings")));
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
                itemListBox.Items[currentItemIndex] = new ApiItemListOption(editingItems[currentItemIndex], currentItemIndex, Localization.Text(Config, "UnitSeconds"), Localization.Text(Config, "GeneralSettings"));
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

            UpdateTimingControls();
            UpdateItemTypeUi();
        }

        private void ItemUseGlobalTimingCheckBoxChanged(object sender, EventArgs e)
        {
            UpdateTimingControls();
        }

        private void UpdateTimingControls()
        {
            bool enabled = itemUseGlobalTimingCheckBox != null &&
                itemUseGlobalTimingCheckBox.Enabled &&
                !itemUseGlobalTimingCheckBox.Checked;
            if (itemIntervalNumeric != null)
            {
                itemIntervalNumeric.Enabled = enabled;
            }

            if (itemTimeoutNumeric != null)
            {
                itemTimeoutNumeric.Enabled = enabled;
            }
        }

        private ApiItemConfig CreateBlankItem()
        {
            ApiItemConfig item = new ApiItemConfig();
            item.Type = ApiItemConfig.TypeCustomApi;
            item.Id = "item" + (editingItems.Count + 1).ToString();
            item.Name = Localization.Text(Config, "NewItemName");
            item.Method = "GET";
            item.Template = "${$.value}";
            item.IntervalSeconds = 0;
            item.TimeoutSeconds = 0;
            item.Headers["Accept"] = "application/json";
            return item;
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

        private Label AddInputLabel(Control parent, string text, int left, int width, Control input)
        {
            Label label = AddLabel(parent, text, left, input.Top, width);
            inputLabelBindings.Add(new InputLabelBinding(label, input, false));
            AlignLabelWithInput(label, input, false);
            return label;
        }

        private Label AddMultilineInputLabel(Control parent, string text, int left, int width, Control input)
        {
            Label label = AddLabel(parent, text, left, input.Top, width);
            inputLabelBindings.Add(new InputLabelBinding(label, input, true));
            AlignLabelWithInput(label, input, true);
            return label;
        }

        private Label AddUnitLabel(Control parent, string text, Control input)
        {
            Label label = AddInputLabel(parent, text, input.Right + 8, 58, input);
            label.ForeColor = SystemColors.GrayText;
            return label;
        }

        private void AlignLabelWithInput(Label label, Control input)
        {
            AlignLabelWithInput(label, input, false);
        }

        private void AlignLabelWithInput(Label label, Control input, bool alignWithFirstLine)
        {
            int referenceHeight = input.Height;
            if (alignWithFirstLine)
            {
                int lineHeight = TextRenderer.MeasureText("Ag", input.Font).Height + 6;
                referenceHeight = Math.Min(input.Height, lineHeight);
            }

            label.Height = Math.Max(1, referenceHeight);
            label.Top = input.Top;
        }

        private void AlignInputLabels()
        {
            for (int i = 0; i < inputLabelBindings.Count; i++)
            {
                InputLabelBinding binding = inputLabelBindings[i];
                if (binding.Label != null && binding.Input != null)
                {
                    AlignLabelWithInput(binding.Label, binding.Input, binding.AlignWithFirstLine);
                }
            }
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

        private RadioButton AddRadioButton(Control parent, string text, int left, int top, int width, bool isChecked)
        {
            RadioButton radioButton = new RadioButton();
            radioButton.Text = text;
            radioButton.Left = left;
            radioButton.Top = top;
            radioButton.Width = width;
            radioButton.Height = 24;
            radioButton.Checked = isChecked;
            parent.Controls.Add(radioButton);
            return radioButton;
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

        private ComboBox AddCurrencyComboBox(Control parent, int left, int top, string currency)
        {
            ComboBox comboBox = new WheelSafeComboBox();
            comboBox.Left = left;
            comboBox.Top = top;
            comboBox.Width = 86;
            comboBox.DropDownStyle = ComboBoxStyle.DropDown;
            comboBox.Items.Add("USD");
            comboBox.Items.Add("CNY");
            comboBox.Items.Add("EUR");
            comboBox.Items.Add("JPY");
            comboBox.Items.Add("HKD");
            comboBox.Items.Add("GBP");
            comboBox.Text = String.IsNullOrWhiteSpace(currency) ? "USD" : currency.Trim().ToUpperInvariant();
            parent.Controls.Add(comboBox);
            return comboBox;
        }

        private ComboBox AddDisplayFormatComboBox(Control parent, int left, int top, string template)
        {
            ComboBox comboBox = new WheelSafeComboBox();
            comboBox.Left = left;
            comboBox.Top = top;
            comboBox.Width = 260;
            comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox.Items.Add(new DisplayFormatOption(Localization.Text(Config, "DisplayFormatSimple"), "{items}"));
            comboBox.Items.Add(new DisplayFormatOption(Localization.Text(Config, "DisplayFormatWithTime"), "{time}  {items}"));
            comboBox.Items.Add(new DisplayFormatOption(Localization.Text(Config, "DisplayFormatWithDateTime"), "{date} {time}  {items}"));
            comboBox.Items.Add(new DisplayFormatOption(Localization.Text(Config, "DisplayFormatWithCount"), "{items}  ({count})"));
            comboBox.Items.Add(new DisplayFormatOption(Localization.Text(Config, "DisplayFormatCustom"), null));
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

        private string GetDisplayTemplate()
        {
            return String.IsNullOrWhiteSpace(displayTemplateTextBox.Text) ? "{items}" : displayTemplateTextBox.Text;
        }

        private void SelectDisplayFormatForTemplate(string template)
        {
            if (displayFormatComboBox == null)
            {
                return;
            }

            string normalized = String.IsNullOrWhiteSpace(template) ? "{items}" : template;
            int customIndex = displayFormatComboBox.Items.Count - 1;
            for (int i = 0; i < displayFormatComboBox.Items.Count; i++)
            {
                DisplayFormatOption option = displayFormatComboBox.Items[i] as DisplayFormatOption;
                if (option != null && option.Template != null && String.Equals(option.Template, normalized, StringComparison.Ordinal))
                {
                    SetDisplayFormatIndex(i);
                    return;
                }
            }

            SetDisplayFormatIndex(customIndex);
        }

        private void SetDisplayFormatIndex(int index)
        {
            if (displayFormatComboBox.SelectedIndex == index)
            {
                return;
            }

            updatingDisplayTemplate = true;
            try
            {
                displayFormatComboBox.SelectedIndex = index;
            }
            finally
            {
                updatingDisplayTemplate = false;
            }
        }

        private void InsertDisplayToken(string token)
        {
            int start = displayTemplateTextBox.SelectionStart;
            displayTemplateTextBox.Text = displayTemplateTextBox.Text.Insert(start, token);
            displayTemplateTextBox.SelectionStart = start + token.Length;
            displayTemplateTextBox.Focus();
        }

        private void UpdateDisplayTemplateUi()
        {
            if (displayPreviewLabel == null)
            {
                return;
            }

            bool custom = IsCustomDisplayFormatSelected();
            SetDisplayTemplateAdvancedVisible(custom);

            List<string> previewParts = CreateDisplayPreviewParts();
            string previewItems = String.Join(PreviewSeparator(), previewParts.ToArray());
            string preview = AppConfig.DecodeTextEscapes(GetDisplayTemplate())
                .Replace("{items}", previewItems)
                .Replace("{count}", previewParts.Count.ToString())
                .Replace("{date}", DateTime.Now.ToString("yyyy-MM-dd"))
                .Replace("{time}", DateTime.Now.ToString("HH:mm:ss"));
            displayPreviewLabel.Text = Localization.Text(Config, "DisplayPreview") + " " + preview;
            LayoutDisplaySettings(custom);
        }

        private List<string> CreateDisplayPreviewParts()
        {
            List<string> parts = new List<string>();
            for (int i = 0; i < editingItems.Count; i++)
            {
                ApiItemConfig item = PreviewItemAt(i);
                if (item == null || !item.Enabled)
                {
                    continue;
                }

                parts.Add(CreateItemPreviewText(item, i));
            }

            if (parts.Count == 0)
            {
                parts.Add("--");
            }

            return parts;
        }

        private ApiItemConfig PreviewItemAt(int index)
        {
            if (index < 0 || index >= editingItems.Count)
            {
                return null;
            }

            ApiItemConfig item = editingItems[index];
            if (index != currentItemIndex || itemTemplateTextBox == null || itemNameTextBox == null ||
                itemIdTextBox == null || itemEnabledCheckBox == null)
            {
                return item;
            }

            ApiItemConfig preview = CloneItem(item);
            preview.Enabled = itemEnabledCheckBox.Checked;
            preview.Name = itemNameTextBox.Text.Trim();
            preview.Id = itemIdTextBox.Text.Trim();
            preview.Type = ApiItemConfig.TypeCustomApi;
            preview.Template = itemTemplateTextBox.Text.Trim();
            AppConfig.ApplyItemTypeDefaults(preview);
            return preview;
        }

        private static string CreateItemPreviewText(ApiItemConfig item, int index)
        {
            if (item == null)
            {
                return "--";
            }

            string type = ApiItemConfig.NormalizeType(item.Type);
            if (type == ApiItemConfig.TypeCoin)
            {
                string symbol = String.IsNullOrWhiteSpace(item.Symbol) ? item.DisplayName(index) : item.Symbol;
                string quote = String.IsNullOrWhiteSpace(item.QuoteCurrency) ? "USD" : item.QuoteCurrency;
                string mark = quote == "USD" ? "$" : quote + " ";
                return symbol.ToUpperInvariant() + " " + mark + "65000.00 (+1.20%)";
            }

            if (type == ApiItemConfig.TypeExchangeRate)
            {
                string baseCurrency = String.IsNullOrWhiteSpace(item.BaseCurrency) ? "USD" : item.BaseCurrency;
                string quoteCurrency = String.IsNullOrWhiteSpace(item.QuoteCurrency) ? "CNY" : item.QuoteCurrency;
                return baseCurrency + "/" + quoteCurrency + " 7.1800";
            }

            if (type == ApiItemConfig.TypeHttpStatus)
            {
                return item.DisplayName(index) + " OK 128ms";
            }

            if (String.IsNullOrWhiteSpace(item.Template))
            {
                return item.DisplayName(index) + ": --";
            }

            try
            {
                string rendered = JsonTemplateRenderer.Render("{}", AppConfig.DecodeTextEscapes(item.Template));
                return String.IsNullOrWhiteSpace(rendered) ? item.DisplayName(index) + ": --" : rendered;
            }
            catch
            {
                return item.DisplayName(index) + ": --";
            }
        }

        private bool IsCustomDisplayFormatSelected()
        {
            DisplayFormatOption option = displayFormatComboBox == null ? null : displayFormatComboBox.SelectedItem as DisplayFormatOption;
            return option != null && option.Template == null;
        }

        private void SetDisplayTemplateAdvancedVisible(bool visible)
        {
            displayTemplateLabel.Visible = visible;
            displayTemplateTextBox.Visible = visible;
            insertItemsButton.Visible = visible;
            insertTimeButton.Visible = visible;
            insertDateButton.Visible = visible;
            insertCountButton.Visible = visible;
            displayTemplateHintLabel.Visible = visible;
        }

        private void LayoutDisplaySettings(bool custom)
        {
            int top = displayGroupTopPadding;
            int labelLineHeight = DisplayTextHeight(displayPreviewLabel);
            int hintHeight = Math.Max(labelLineHeight, DisplayTextHeight(displayTemplateHintLabel));
            SetInputRow(displayFormatComboBox, top);
            top = displayFormatComboBox.Bottom + displayRowGap;

            if (custom)
            {
                SetInputRow(displayTemplateTextBox, top);
                AlignLabelWithInput(displayTemplateLabel, displayTemplateTextBox);
                top = displayTemplateTextBox.Bottom + displayRowGap;

                SetButtonRow(top);
                top = insertItemsButton.Bottom + displayHintGap;

                displayTemplateHintLabel.Left = displayInputLeft;
                displayTemplateHintLabel.Top = top;
                displayTemplateHintLabel.Width = 560;
                displayTemplateHintLabel.Height = hintHeight;
                top = displayTemplateHintLabel.Bottom + displayBlockGap;
            }

            SetInputRow(itemSeparatorTextBox, top);
            AlignLabelWithInput(itemSeparatorLabel, itemSeparatorTextBox);
            top = itemSeparatorTextBox.Bottom + displayHintGap;

            itemSeparatorHintLabel.Left = displayInputLeft;
            itemSeparatorHintLabel.Top = top;
            itemSeparatorHintLabel.Width = 560;
            itemSeparatorHintLabel.Height = hintHeight;
            top = itemSeparatorHintLabel.Bottom + displayBlockGap;

            displayPreviewLabel.Left = displayInputLeft;
            displayPreviewLabel.Top = top;
            displayPreviewLabel.Width = 560;
            displayPreviewLabel.Height = Math.Max(labelLineHeight, DisplayPreviewTextHeight());
            top = displayPreviewLabel.Bottom + displayBottomPadding;

            displayGroup.Height = top;
        }

        private int DisplayTextHeight(Control control)
        {
            int measured = TextRenderer.MeasureText("预览：BTC 65000", control.Font).Height;
            return Math.Max(control.Font.Height + 8, measured + 8);
        }

        private int DisplayPreviewTextHeight()
        {
            string text = String.IsNullOrEmpty(displayPreviewLabel.Text) ? "Preview" : displayPreviewLabel.Text;
            int width = displayPreviewLabel.Width > 0 ? displayPreviewLabel.Width : 560;
            Size measured = TextRenderer.MeasureText(text, displayPreviewLabel.Font, new Size(width, 0), TextFormatFlags.WordBreak);
            return Math.Max(displayPreviewLabel.Font.Height + 8, measured.Height + 8);
        }

        private void CaptureDisplayLayoutBaselines()
        {
            float scale = Math.Max(1F, displayFormatComboBox.Height / 21F);
            displayGroupTopPadding = ScaleValue(30, scale);
            displayLabelLeft = ScaleValue(14, scale);
            displayInputLeft = ScaleValue(180, scale);
            displayLabelWidth = ScaleValue(150, scale);
            displayRowGap = ScaleValue(10, scale);
            displayHintGap = ScaleValue(8, scale);
            displayBlockGap = ScaleValue(18, scale);
            displayBottomPadding = ScaleValue(18, scale);
            displayTemplateLabel.Left = displayLabelLeft;
            itemSeparatorLabel.Left = displayLabelLeft;
            displayTemplateLabel.Width = displayLabelWidth;
            itemSeparatorLabel.Width = displayLabelWidth;
        }

        private void SetInputRow(Control input, int top)
        {
            input.Left = displayInputLeft;
            input.Top = top;
        }

        private void SetButtonRow(int top)
        {
            int left = displayInputLeft;
            int width = ScaleValue(72, Math.Max(1F, displayFormatComboBox.Height / 21F));
            int gap = ScaleValue(8, Math.Max(1F, displayFormatComboBox.Height / 21F));
            Button[] buttons = new Button[] { insertItemsButton, insertTimeButton, insertDateButton, insertCountButton };
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Left = left + i * (width + gap);
                buttons[i].Top = top;
                buttons[i].Width = width;
            }
        }

        private string PreviewSeparator()
        {
            if (itemSeparatorTextBox == null)
            {
                return "   ";
            }

            return AppConfig.DecodeTextEscapes(itemSeparatorTextBox.Text);
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
            item.Type = source.Type;
            item.Symbol = source.Symbol;
            item.BaseCurrency = source.BaseCurrency;
            item.QuoteCurrency = source.QuoteCurrency;
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
                CaptureDisplayLayoutBaselines();
                UpdateDisplayTemplateUi();
                AlignInputLabels();
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
            private readonly string generalText;

            public ApiItemListOption(ApiItemConfig item, int index, string secondsUnit, string generalText)
            {
                this.item = item;
                this.index = index;
                this.secondsUnit = secondsUnit;
                this.generalText = generalText;
            }

            public override string ToString()
            {
                string name = item == null ? "" : item.DisplayName(index);
                string mark = item != null && item.Enabled ? "[x] " : "[ ] ";
                if (item == null)
                {
                    return mark + name;
                }

                if (item.IntervalSeconds <= 0 && item.TimeoutSeconds <= 0)
                {
                    return mark + name + " (" + generalText + ")";
                }

                return mark + name + " (" + FormatTiming(item.IntervalSeconds) + " / " +
                    FormatTiming(item.TimeoutSeconds) + ")";
            }

            private string FormatTiming(int seconds)
            {
                return seconds > 0 ? seconds.ToString() + " " + secondsUnit : generalText;
            }
        }

        private sealed class InputLabelBinding
        {
            public readonly Label Label;
            public readonly Control Input;
            public readonly bool AlignWithFirstLine;

            public InputLabelBinding(Label label, Control input, bool alignWithFirstLine)
            {
                Label = label;
                Input = input;
                AlignWithFirstLine = alignWithFirstLine;
            }
        }

        private sealed class DisplayFormatOption
        {
            public readonly string Name;
            public readonly string Template;

            public DisplayFormatOption(string name, string template)
            {
                Name = name;
                Template = template;
            }

            public override string ToString()
            {
                return Name;
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
