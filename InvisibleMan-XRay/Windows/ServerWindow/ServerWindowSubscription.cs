using System;
using System.Linq;
using System.Windows;
using System.Collections.Generic;

namespace InvisibleManXRay
{
    using Models;
    using Values;
    using Utilities;

    public partial class ServerWindow : Window
    {
        private enum SubscriptionOperation { CREATE, EDIT }

        private string groupPath = null;
        private SubscriptionOperation subscriptionOperation;
        private List<Components.Config> subscriptionConfigComponents;

        private Func<string, List<Config>> getAllSubscriptionConfigs;
        private Func<List<Subscription>> getAllGroups;
        private Func<string, Status> convertLinkToSubscription;
        private Action<SubscriptionInfo> onCreateSubscription;
        private Action<Subscription> onDeleteSubscription;

        public void OpenImportSubscriptionWithLinkSection(string link)
        {
            pendingToRenderActions = () =>
            {
                OnSubscriptionTabClick(null, null);
                OnAddSubscriptionButtonClick(null, null);
                textBoxSubscriptionLink.Text = link;
            };
        }

        private void InitializeImportingGroups()
        {
            SetActiveFileImportingGroup(true);
            SetActiveLinkImportingGroup(false);
            SetImportingType(ImportingType.FILE);
        }

        private void InitializeSubscriptionConfigComponents()
        {
            subscriptionConfigComponents = new List<Components.Config>();
        }

        private void InitializeGroupPath()
        {
            groupPath = getCurrentConfigPath.Invoke();
        }

        private void OnSubscriptionTabClick(object sender, RoutedEventArgs e)
        {
            EnableAllTabs();
            HideAllPanels();

            SetEnableSubscriptionTabButton(false);
            SetActiveSubscriptionPanel(true);
        }

        private void OnSubscriptionComboBoxSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (comboBoxSubscription.SelectedValue == null)
                return;

            groupPath = ((Subscription)comboBoxSubscription.SelectedValue).Directory.FullName;
            LoadConfigsList(GroupType.SUBSCRIPTION);
            SelectConfig(getCurrentConfigPath.Invoke());
        }

        private void OnAddSubscriptionButtonClick(object sender, RoutedEventArgs e)
        {
            ShowAddSubscriptionsServerPanel();
        }

        private void OnDeleteSubscriptionButtonClick(object sender, RoutedEventArgs e)
        {
            if (comboBoxSubscription.SelectedValue == null)
                return;

            string remarks = comboBoxSubscription.Text;
            Subscription subscription = (Subscription)comboBoxSubscription.SelectedValue;

            MessageBoxResult result = MessageBox.Show(
                this,
                string.Format(LocalizationService.GetTerm(Localization.DELETE_CONFIRMATION), remarks),
                Caption.INFO,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
                DeleteSubscription(subscription);
        }

        private void OnEditSubscriptionButtonClick(object sender, RoutedEventArgs e)
        {
            if (comboBoxSubscription.SelectedValue == null)
                return;

            ShowEditSubscriptionServerPanel();
        }

        private void OnUpdateSubscriptionButtonClick(object sender, RoutedEventArgs e)
        {
            InitializeTextBoxFields();
            UpdateSubscription();

            void InitializeTextBoxFields()
            {
                textBoxSubscriptionLink.Text = ((Subscription)comboBoxSubscription.SelectedValue).Info.Url;
            }

            void UpdateSubscription()
            {
                EditSubscription(
                    subscription: (Subscription)comboBoxSubscription.SelectedValue,
                    link: textBoxSubscriptionLink.Text
                );
            }
        }

        private void OnImportSubscriptionButtonClick(object sender, RoutedEventArgs e)
        {
            if (subscriptionOperation == SubscriptionOperation.CREATE)
            {
                HandleImportingSubscription();
            }
            else if (subscriptionOperation == SubscriptionOperation.EDIT)
            {
                EditSubscription(
                    subscription: (Subscription)comboBoxSubscription.SelectedValue,
                    link: textBoxSubscriptionLink.Text
                );
            }
        }

        private void HandleImportingSubscription()
        {
            if (!IsLinkEntered())
            {
                MessageBox.Show(
                    this,
                    LocalizationService.GetTerm(Values.Localization.NO_SUBSCRIPTION_LINK_ENTERED),
                    Values.Caption.WARNING,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            SetActiveLoadingPanel(true);
            TryAddSubscription();

            bool IsLinkEntered() => !string.IsNullOrEmpty(textBoxSubscriptionLink.Text);

            void TryAddSubscription()
            {
                Status subscriptionStatus;

                subscriptionStatus = convertLinkToSubscription.Invoke(
                    textBoxSubscriptionLink.Text
                );

                if (subscriptionStatus.Code == Code.ERROR)
                {
                    HandleError();
                    SetActiveLoadingPanel(false);
                    return;
                }

                var subscription = (SubscriptionInfo)subscriptionStatus.Content;
                groupPath = "";
                onCreateSubscription.Invoke(
                    subscription
                );
                onUpdateConfig.Invoke(GetLastConfigPath(GroupType.SUBSCRIPTION));
                SetActiveLoadingPanel(false);
                LoadGroupsList();
                LoadConfigsList(GroupType.SUBSCRIPTION);
                ClearSubscriptionPath();
                ShowServersPanel();

                void HandleError()
                {
                    switch (subscriptionStatus.SubCode)
                    {
                        case SubCode.NO_CONFIG:
                        case SubCode.UNSUPPORTED_LINK:
                            HandleWarningMessage(subscriptionStatus.Content.ToString());
                            break;
                        case SubCode.INVALID_CONFIG:
                            HandleErrorMessage(subscriptionStatus.Content.ToString());
                            break;
                        default:
                            return;
                    }
                }
            }
        }

        private void EditSubscription(Subscription subscription, string link)
        {
            string oldRemarks = comboBoxSubscription.Text;

            HandleImportingSubscription();
            if (!HasSameRemarks())
                DeleteSubscription(subscription);

            bool HasSameRemarks() => oldRemarks == comboBoxSubscription.Text;
        }

        private void ShowAddSubscriptionsServerPanel()
        {
            subscriptionOperation = SubscriptionOperation.CREATE;
            ShowSubscriptionPanel();
            InitializeTextBoxFields();

            void ShowSubscriptionPanel()
            {
                panelServers.Visibility = Visibility.Hidden;
                panelAddSubscriptions.Visibility = Visibility.Visible;
            }

            void InitializeTextBoxFields()
            {
                textBoxSubscriptionLink.Text = "";
            }
        }

        private void ShowEditSubscriptionServerPanel()
        {
            subscriptionOperation = SubscriptionOperation.EDIT;
            ShowSubscriptionPanel();
            FetchTextBoxFields();

            void ShowSubscriptionPanel()
            {
                panelServers.Visibility = Visibility.Hidden;
                panelAddSubscriptions.Visibility = Visibility.Visible;
            }

            void FetchTextBoxFields()
            {
                textBoxSubscriptionLink.Text = ((Subscription)comboBoxSubscription.SelectedValue).Info.Url;
            }
        }


        private void ClearSubscriptionPath()
        {
            textBoxSubscriptionLink.Text = null;
        }

        private void LoadGroupsList()
        {
            Dictionary<Subscription, string> groups;
            InitializeGroups();
            SelectCurrentGroup();

            void InitializeGroups()
            {
                groups = new Dictionary<Subscription, string>();
                foreach (Subscription group in getAllGroups.Invoke())
                    groups.Add(group, group.Name);

                comboBoxSubscription.ItemsSource = groups;
            }

            void SelectCurrentGroup()
            {
                if (!IsAnyGroupExists())
                    return;

                KeyValuePair<Subscription, string> currentGroup = groups.FirstOrDefault(
                    group => group.Key.Directory.FullName == FileUtility.GetDirectory(groupPath)
                );

                if (!IsCurrentGroupExists())
                    currentGroup = groups.Last();

                comboBoxSubscription.SelectedValue = currentGroup.Key;

                bool IsCurrentGroupExists() => currentGroup.Key != null && currentGroup.Value != null;
            }

            bool IsAnyGroupExists() => groups.Count > 0;
        }

        private void LoadSubscriptionConfigsList()
        {
            List<Config> configs = getAllSubscriptionConfigs.Invoke(groupPath);
            List<Subscription> groups = getAllGroups.Invoke();
            subscriptionConfigComponents = new List<Components.Config>();

            ClearConfigsList(listSubscriptions);
            HandleShowingSubscriptionControlPanel();

            HandleShowingNoServerExistsHint(
                configs: configs,
                groups: groups,
                textNoServer: textNoSubscription
            );

            AddAllConfigsToList(
                configs: configs,
                configComponents: subscriptionConfigComponents,
                parent: listSubscriptions
            );

            if (IsAnyConfigExists(configs))
                AddConfigHintAtTheEndOfList(listSubscriptions);

            void HandleShowingSubscriptionControlPanel()
            {
                if (groups.Count > 0)
                    panelSubscriptionControl.Visibility = Visibility.Visible;
                else
                    panelSubscriptionControl.Visibility = Visibility.Collapsed;
            }
        }

        private string GetLastSubscriptionConfigPath()
        {
            Config lastConfig = getAllSubscriptionConfigs.Invoke(groupPath).LastOrDefault();

            if (!IsConfigExists())
                lastConfig = getAllGeneralConfigs.Invoke().LastOrDefault();

            if (!IsConfigExists())
                return null;

            return lastConfig.Path;

            bool IsConfigExists() => lastConfig != null;
        }

        private void SetActiveSubscriptionPanel(bool isActive) => SetActivePanel(panelSubscription, isActive);

        private void SetEnableSubscriptionTabButton(bool isEnabled) => SetEnableButton(buttonSubscriptionTab, isEnabled);
    }
}