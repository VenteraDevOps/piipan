using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Bunit;
using Piipan.Components.Alerts;
using Piipan.Components.Forms;
using Piipan.Components.Modals;
using Piipan.Match.Api.Models;
using Piipan.QueryTool.Client.Components;
using Piipan.QueryTool.Client.Components.UnauthorizedBanners;
using Piipan.QueryTool.Client.Models;
using Xunit;
using static Piipan.Components.Forms.FormConstants;
using static Piipan.Components.Validation.ValidationConstants;

namespace Piipan.QueryTool.Tests.Components
{
    public class QueryFormTests : BaseComponentTest<QueryForm>
    {
        #region Tests
        PiiQuery model = new PiiQuery();
        bool invalidDateInput = false;
        ComponentParameterCollectionBuilder<QueryForm> parameters = new ComponentParameterCollectionBuilder<QueryForm>();
        IElement? inputElement = null;

        public QueryFormTests()
        {
            InitialValues.QueryFormData = new QueryFormModel
            {
                Query = model,
                Location = "EA"
            };
        }

        /// <summary>
        /// Verify that when the value changes, the backend model is updated
        /// </summary>
        [Fact]
        public void Form_Should_Bind_Value_On_Change()
        {
            // Arrange
            CreateTestComponent();

            // Act
            Component.Find("#QueryFormData_Query_LastName").Change("Name");
            Component.Find("#QueryFormData_Query_DateOfBirth").Change("1997-01-01");
            Component.Find("#QueryFormData_Query_SocialSecurityNum").Input("550-01-6981");
            Component.Find("#QueryFormData_Query_ParticipantId").Change("123");
            Component.Find("#QueryFormData_Query_CaseId").Change("456");
            inputElement.Change("recertification");

            // Assert
            Assert.Equal("Name", model.LastName);
            Assert.Equal(DateTime.Parse("1997-01-01"), model.DateOfBirth);
            Assert.Equal("550-01-6981", model.SocialSecurityNum);
            Assert.Equal("123", model.ParticipantId);
            Assert.Equal("456", model.CaseId);
            Assert.Equal("recertification", model.SearchReason);
        }

        /// <summary>
        /// Verify that when there are no results returned that we get an informational alert
        /// </summary>
        [Fact]
        public void Verify_No_Results_Alert_Box_No_Results()
        {
            // Arrange

            // Add a result with no matches
            InitialValues.QueryFormData.QueryResult = new()
            {
                Results = new()
            };
            CreateTestComponent();

            // Assert
            var alertBox = Component.FindComponent<UsaAlertBox>();
            Assert.Contains("This participant does not have a matching record in any other states.", alertBox.Markup);
            Assert.False(Component.HasComponent<QueryResults>());
        }

        /// <summary>
        /// Verify that when there are no matches in the returned result that we get an informational alert
        /// </summary>
        [Fact]
        public void Verify_No_Results_Alert_Box_No_Matches()
        {
            // Arrange

            // Add a result with no matches
            InitialValues.QueryFormData.QueryResult = new()
            {
                Results = new()
                {
                    new()
                    {
                        Index = 1,
                        Matches = new List<ParticipantMatch>()
                    }
                }
            };
            CreateTestComponent();

            // Assert
            var alertBox = Component.FindComponent<UsaAlertBox>();
            Assert.Contains("This participant does not have a matching record in any other states.", alertBox.Markup);
            Assert.False(Component.HasComponent<QueryResults>());
        }

        /// <summary>
        /// Verify that if we have results that they are shown on the screen
        /// </summary>
        [Fact]
        public void Verify_Results_Shown()
        {
            // Arrange

            // Add a result with no matches
            InitialValues.QueryFormData.QueryResult = new()
            {
                Results = new()
                {
                    new()
                    {
                        Index = 1,
                        Matches = new List<ParticipantMatch>()
                        {
                            new ()
                            {
                                MatchId = "1234",
                                State = "ea"
                            }
                        }
                    }
                }
            };
            CreateTestComponent();

            // Assert
            Assert.True(Component.HasComponent<QueryResults>());
            Assert.False(Component.HasComponent<UsaAlertBox>());
        }

        /// <summary>
        /// Verify that if we have results that has a vulnerable individual, clicking it shows the vulnerable match modal
        /// </summary>
        [Fact]
        public void Verify_VulnerableModal_ShownWhenClicked_VulnerableMatch()
        {
            // Arrange

            // Add a result with no matches
            InitialValues.QueryFormData.QueryResult = new()
            {
                Results = new()
                {
                    new()
                    {
                        Index = 1,
                        Matches = new List<ParticipantMatch>()
                        {
                            new ()
                            {
                                MatchId = "1234",
                                State = "ea",
                                VulnerableIndividual = true
                            }
                        }
                    }
                }
            };
            CreateTestComponent();

            var queryResults = Component.FindComponent<QueryResults>();
            queryResults.Find("a").Click();

            // Assert
            var modalManager = Services.GetService<IModalManager>();
            Assert.True(modalManager.OpenModals.First().ForceAction);
            Assert.Equal(1, modalManager.OpenModals.Count);
        }

        /// <summary>
        /// Verify that if we have results that has do not have a vulnerable individual, clicking it does NOT show the vulnerable match modal
        /// </summary>
        [Fact]
        public void Verify_VulnerableModal_NotShownWhenClicked_NotVulnerableMatch()
        {
            // Arrange

            // Add a result with no matches
            InitialValues.QueryFormData.QueryResult = new()
            {
                Results = new()
                {
                    new()
                    {
                        Index = 1,
                        Matches = new List<ParticipantMatch>()
                        {
                            new ()
                            {
                                MatchId = "1234",
                                State = "ea",
                                VulnerableIndividual = false
                            }
                        }
                    }
                }
            };
            CreateTestComponent();

            var queryResults = Component.FindComponent<QueryResults>();
            queryResults.Find("a").Click();

            // Assert
            var modalManager = Services.GetService<IModalManager>();
            Assert.Empty(modalManager.OpenModals);
        }

        /// <summary>
        /// Verify that when searching a valid form that the button text changes to "Searching..."
        /// </summary>
        [Fact]
        public async Task Button_Should_Say_Searching_While_Searching_Valid_Form()
        {
            // Arrange
            CreateTestComponent();
            var searchButton = Component.Find("#query-form-search-btn");
            var form = Component.FindComponent<UsaForm>();

            // Assert
            Assert.Equal("Search", searchButton.TextContent);
            Assert.False(searchButton.HasAttribute("disabled"));

            // Act
            Component.Find("#QueryFormData_Query_LastName").Change("Name");
            Component.Find("#QueryFormData_Query_DateOfBirth").Change("1997-01-01");
            Component.Find("#QueryFormData_Query_SocialSecurityNum").Input("550-01-6981");
            Component.Find("#QueryFormData_Query_ParticipantId").Change("123");
            inputElement.Change("other");
            bool isFormValid = false;
            await form.InvokeAsync(async () =>
            {
                isFormValid = await form.Instance.ValidateForm();
            });
            await form.InvokeAsync(async () =>
            {
                await form.Instance.OnBeforeSubmit(isFormValid);
            });

            // Assert
            Assert.Equal("Searching...", searchButton.TextContent);
            Assert.True(searchButton.HasAttribute("disabled"));
        }

        /// <summary>
        /// Verify that when searching an invalid form that the button text does not change to "Searching..."
        /// </summary>
        [Fact]
        public async Task Button_Should_Not_Say_Searching_While_Searching_Invalid_Form()
        {
            // Arrange
            CreateTestComponent();
            var searchButton = Component.Find("#query-form-search-btn");
            var form = Component.FindComponent<UsaForm>();

            // Assert
            Assert.Equal("Search", searchButton.TextContent);
            Assert.False(searchButton.HasAttribute("disabled"));

            // Act
            bool isFormValid = false;
            await form.InvokeAsync(async () =>
            {

                isFormValid = await form.Instance.ValidateForm();
            });
            await form.InvokeAsync(async () =>
            {
                await form.Instance.OnBeforeSubmit(isFormValid);
            });

            // Assert
            Assert.Equal("Search", searchButton.TextContent);
            Assert.False(searchButton.HasAttribute("disabled"));
        }

        /// <summary>
        /// Verify that when you don't have a state location that the button is disabled
        /// </summary>
        [Fact]
        public void Form_Should_Be_Disabled_When_Not_Authorized()
        {
            // Arrange
            AppData.IsAuthorized = false;
            CreateTestComponent();

            // Assert
            Assert.True(Component.HasComponent<GenericUnauthorizedBanner>());

            var wrapper = Component.Find("#snap-participants-query-form-wrapper");
            Assert.True(wrapper.ClassList.Contains("disabled-area"));
            Assert.True(wrapper.HasAttribute("inert")); // Inert makes it so the fields cannot be entered
        }

        /// <summary>
        /// Verify that when the server has an error we display it in an alert box on the screen
        /// </summary>
        [Fact]
        public void Form_With_Server_Error_Should_Show_Errors()
        {
            // Arrange
            InitialValues.QueryFormData.ServerErrors = new List<ServerError>
            {
                new("QueryFormData.Query.LastName", $"{ValidationFieldPlaceholder} is required")
            };
            CreateTestComponent();

            var alertBox = Component.FindComponent<UsaAlertBox>();
            var alertBoxErrors = alertBox.FindAll("li");

            // Assert
            Assert.Equal(1, alertBoxErrors.Count);

            List<string> errors = new List<string>
            {
                "Last Name is required"
            };

            for (int i = 0; i < alertBoxErrors.Count; i++)
            {
                string error = alertBoxErrors[i].TextContent.Replace("\n", "");
                error = Regex.Replace(error, @"\s+", " ");
                Assert.Contains(errors[i], error);
            }
        }

        /// <summary>
        /// Verify that when there are required field errors on the screen that they are all shown in the alert box
        /// and above the field
        /// </summary>
        [Fact]
        public async Task Form_Should_Show_Required_Errors()
        {
            // Arrange
            CreateTestComponent();
            var form = Component.FindComponent<UsaForm>();

            // Act
            bool isFormValid = false;
            await form.InvokeAsync(async () =>
            {
                isFormValid = await form.Instance.ValidateForm();
            });
            await form.InvokeAsync(async () =>
            {
                await form.Instance.OnBeforeSubmit(isFormValid);
            });
            var alertBox = Component.FindComponent<UsaAlertBox>();
            var alertBoxErrors = alertBox.FindAll("li");
            var inputErrorMessages = form.FindAll($".{InputErrorMessageClass}");

            // Assert
            Assert.False(isFormValid);
            Assert.Equal(5, alertBoxErrors.Count);
            Assert.Equal(5, inputErrorMessages.Count);

            List<string> errors = new List<string>
            {
                "Last Name is required",
                "Date of Birth is required",
                "Social Security Number is required",
                "Participant ID is required",
                "Search Reason is required"
            };

            for (int i = 0; i < alertBoxErrors.Count; i++)
            {
                string error = alertBoxErrors[i].TextContent.Replace("\n", "");
                error = Regex.Replace(error, @"\s+", " ");
                Assert.Contains(errors[i], error);
            }

            for (int i = 0; i < inputErrorMessages.Count; i++)
            {
                string error = inputErrorMessages[i].TextContent.Replace("\n", "");
                error = Regex.Replace(error, @"\s+", " ");
                Assert.Contains(errors[i], error);
            }
        }

        #endregion Tests

        #region Helper Function

        /// <summary>
        /// Setup the component and register Javascript mocks
        /// </summary>
        protected override void CreateTestComponent()
        {
            base.CreateTestComponent();
            JSInterop.SetupVoid("piipan.utilities.registerFormValidation", _ => true).SetVoidResult();
            JSInterop.Setup<int>("piipan.utilities.getCursorPosition", _ => true).SetResult(1);
            JSInterop.SetupVoid("piipan.utilities.setCursorPosition", _ => true).SetVoidResult();
            JSInterop.SetupVoid("piipan.utilities.scrollToElement", _ => true).SetVoidResult();
            JSInterop.Setup<bool>("piipan.utilities.doesElementHaveInvalidInput", _ => true).SetResult(invalidDateInput);
            Component = RenderComponent<QueryForm>((builder) =>
            {
                builder.Add(n => n.QueryFormData, InitialValues.QueryFormData);
            });
            inputElement = Component.Find($".{InputRadioClass}");
        }


        #endregion Helper Functions
    }
}
