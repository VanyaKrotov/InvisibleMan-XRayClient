using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace InvisibleManXRay
{
    using Models;

    public partial class SettingsWindow : Window
    {
        private static readonly Dictionary<string, string> Languages = new Dictionary<string, string>() {
            { "en-US", "English" },
            { "ru-RU", "Русский" },
            { "fa-IR", "فارسی" }
        };

        private static readonly Dictionary<Mode, string> Modes = new Dictionary<Mode, string>() {
            { Mode.PROXY, "Proxy" },
            { Mode.TUN, "TUN" }
        };

        private static readonly Dictionary<Protocol, string> Protocols = new Dictionary<Protocol, string>() {
            { Protocol.HTTP, "http" },
            { Protocol.SOCKS, "socks" }
        };

        private static readonly Dictionary<LogLevel, string> logLevels = new Dictionary<LogLevel, string>() {
            { LogLevel.NONE, "None" },
            { LogLevel.DEBUG, "Debug" },
            { LogLevel.INFO, "Info"},
            { LogLevel.WARNING, "Warning" },
            { LogLevel.ERROR, "Error" }
        };

        private Func<string> getLanguage;
        private Func<Mode> getMode;
        private Func<Protocol> getProtocol;
        private Func<bool> getSystemProxyUsed;
        private Func<bool> getUdpEnabled;
        private Func<bool> getRunningAtStartupEnabled;
        private Func<bool> getStartHiddenEnabled;
        private Func<bool> getAutoConnectEnabled;
        private Func<int> getProxyPort;
        private Func<int> getTunPort;
        private Func<int> getTestPort;
        private Func<string> getDeviceIp;
        private Func<string> getDns;
        private Func<LogLevel> getLogLevel;
        private Func<string> getLogPath;

        private Action<UserSettings> onUpdateUserSettings;

        public SettingsWindow()
        {
            InitializeComponent();
            InitializeItems();

            void InitializeItems()
            {
                InitializeLanguageItems();
                InitializeModeItems();
                InitializeProtocolItems();
                InitializeLogLevelItems();

                void InitializeLanguageItems() => comboBoxLanguage.ItemsSource = Languages;

                void InitializeModeItems() => comboBoxMode.ItemsSource = Modes;

                void InitializeProtocolItems() => comboBoxProtocol.ItemsSource = Protocols;

                void InitializeLogLevelItems() => comboBoxLogLevel.ItemsSource = logLevels;
            }
        }

        public void Setup(
            Func<string> getLanguage,
            Func<Mode> getMode,
            Func<Protocol> getProtocol,
            Func<bool> getSystemProxyUsed,
            Func<bool> getUdpEnabled,
            Func<bool> getRunningAtStartupEnabled,
            Func<bool> getStartHiddenEnabled,
            Func<bool> getAutoConnectEnabled,
            Func<int> getProxyPort,
            Func<int> getTunPort,
            Func<int> getTestPort,
            Func<string> getDeviceIp,
            Func<string> getDns,
            Func<LogLevel> getLogLevel,
            Func<string> getLogPath,
            Action<UserSettings> onUpdateUserSettings
        )
        {
            this.getLanguage = getLanguage;
            this.getMode = getMode;
            this.getProtocol = getProtocol;
            this.getSystemProxyUsed = getSystemProxyUsed;
            this.getUdpEnabled = getUdpEnabled;
            this.getRunningAtStartupEnabled = getRunningAtStartupEnabled;
            this.getStartHiddenEnabled = getStartHiddenEnabled;
            this.getAutoConnectEnabled = getAutoConnectEnabled;
            this.getProxyPort = getProxyPort;
            this.getTunPort = getTunPort;
            this.getTestPort = getTestPort;
            this.getDeviceIp = getDeviceIp;
            this.getDns = getDns;
            this.getLogLevel = getLogLevel;
            this.getLogPath = getLogPath;
            this.onUpdateUserSettings = onUpdateUserSettings;

            UpdateUI();
        }

        private void UpdateUI()
        {
            UpdateBasicPanelUI();
            UpdatePortPanelUI();
            UpdateTunPanelUI();
            UpdateLogPanelUI();

            void UpdateBasicPanelUI()
            {
                comboBoxLanguage.SelectedValue = getLanguage.Invoke();
                comboBoxMode.SelectedValue = getMode.Invoke();
                comboBoxProtocol.SelectedValue = getProtocol.Invoke();
                checkBoxUseSystemProxy.IsChecked = getSystemProxyUsed.Invoke();
                checkBoxEnableUdp.IsChecked = getUdpEnabled.Invoke();
                checkBoxRunAtStartup.IsChecked = getRunningAtStartupEnabled.Invoke();
                checkBoxStartHidden.IsChecked = getStartHiddenEnabled.Invoke();
                checkBoxAutoConnect.IsChecked = getAutoConnectEnabled.Invoke();
            }

            void UpdatePortPanelUI()
            {
                textBoxProxyPort.Text = getProxyPort.Invoke().ToString();
                textBoxTunPort.Text = getTunPort.Invoke().ToString();
                textBoxTestPort.Text = getTestPort.Invoke().ToString();
            }

            void UpdateTunPanelUI()
            {
                textBoxTunDeviceIp.Text = getDeviceIp.Invoke();
                textBoxTunDns.Text = getDns.Invoke();
            }

            void UpdateLogPanelUI()
            {
                comboBoxLogLevel.SelectedValue = getLogLevel.Invoke();
                textBoxLogPath.Text = Path.GetFullPath(getLogPath.Invoke());
            }
        }

        private void OnBasicTabClick(object sender, RoutedEventArgs e)
        {
            EnableAllTabs();
            HideAllPanels();

            SetEnableBasicTabButton(false);
            SetActiveBasicPanel(true);
        }

        private void OnPortTabClick(object sender, RoutedEventArgs e)
        {
            EnableAllTabs();
            HideAllPanels();

            SetEnablePortTabButton(false);
            SetActivePortPanel(true);
        }

        private void OnTunTabClick(object sender, RoutedEventArgs e)
        {
            EnableAllTabs();
            HideAllPanels();

            SetEnableTunTabButton(false);
            SetActiveTunPanel(true);
        }

        private void OnLogTabClick(object sender, RoutedEventArgs e)
        {
            EnableAllTabs();
            HideAllPanels();

            SetEnableLogTabButton(false);
            SetActiveLogPanel(true);
        }

        private void OnModeComboBoxSelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdateUIBasedOnMode();

            void UpdateUIBasedOnMode()
            {
                Mode mode = (Mode)comboBoxMode.SelectedValue;
                
                comboBoxProtocol.IsEnabled = mode != Mode.TUN;
                checkBoxUseSystemProxy.IsEnabled = mode != Mode.TUN;
                checkBoxEnableUdp.IsEnabled = mode == Mode.TUN;
            }
        }

        private void OnConfirmButtonClick(object sender, RoutedEventArgs e)
        {
            UserSettings userSettings = new UserSettings(
                language: comboBoxLanguage.SelectedValue.ToString(),
                mode: (Mode)comboBoxMode.SelectedValue,
                protocol: (Protocol)comboBoxProtocol.SelectedValue,
                logLevel: (LogLevel)comboBoxLogLevel.SelectedValue,
                isSystemProxyUse: checkBoxUseSystemProxy.IsChecked.Value,
                isUdpEnable: checkBoxEnableUdp.IsChecked.Value,
                isRunningAtStartup: checkBoxRunAtStartup.IsChecked.Value,
                isStartHidden: checkBoxStartHidden.IsChecked.Value,
                isAutoConnect: checkBoxAutoConnect.IsChecked.Value,
                proxyPort: int.Parse(textBoxProxyPort.Text),
                tunPort: int.Parse(textBoxTunPort.Text),
                testPort: int.Parse(textBoxTestPort.Text),
                tunIp: textBoxTunDeviceIp.Text,
                dns: textBoxTunDns.Text,
                logPath: textBoxLogPath.Text
            );
            
            SendRunAtStartupActivationEvent();
            onUpdateUserSettings.Invoke(userSettings);

            Close();

            void SendRunAtStartupActivationEvent()
            {
                if (!IsUserChangeRunningAtStartupSetting())
                    return;

                bool IsUserChangeRunningAtStartupSetting()
                {
                    return getRunningAtStartupEnabled.Invoke() != checkBoxRunAtStartup.IsChecked.Value;
                }
            }
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SetActiveBasicPanel(bool isActive) => SetActivePanel(panelBasic, isActive);

        private void SetActivePortPanel(bool isActive) => SetActivePanel(panelPort, isActive);

        private void SetActiveTunPanel(bool isActive) => SetActivePanel(panelTun, isActive);

        private void SetActiveLogPanel(bool isActive) => SetActivePanel(panelLog, isActive);

        private void SetActivePanel(UIElement panel, bool isActive)
        {
            panel.Visibility = isActive ? Visibility.Visible : Visibility.Hidden;
        }
        
        private void HideAllPanels()
        {
            SetActiveBasicPanel(false);
            SetActivePortPanel(false);
            SetActiveTunPanel(false);
            SetActiveLogPanel(false);
        }

        private void SetEnableBasicTabButton(bool isEnabled) => SetEnableButton(buttonBasicTab, isEnabled);

        private void SetEnablePortTabButton(bool isEnabled) => SetEnableButton(buttonPortTab, isEnabled);

        private void SetEnableTunTabButton(bool isEnabled) => SetEnableButton(buttonTunTab, isEnabled);

        private void SetEnableLogTabButton(bool isEnabled) => SetEnableButton(buttonLogTab, isEnabled);

        private void SetEnableButton(Button button, bool isEnabled)
        {
            button.IsEnabled = isEnabled;
        }

        private void EnableAllTabs()
        {
            SetEnableBasicTabButton(true);
            SetEnablePortTabButton(true);
            SetEnableTunTabButton(true);
            SetEnableLogTabButton(true);
        }
    }
}