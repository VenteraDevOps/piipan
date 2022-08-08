using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bunit;
using Piipan.Components.Alerts;
using Piipan.Components.Forms;
using Piipan.Components.Modals;
using Piipan.Match.Api.Models.Resolution;
using Piipan.QueryTool.Client.Components;
using Piipan.QueryTool.Client.Models;
using Xunit;
using static Piipan.Components.Forms.FormConstants;
using static Piipan.Components.Validation.ValidationConstants;

namespace Piipan.QueryTool.Tests.Components
{
    public class MatchFormTests : BaseComponentTest<MatchForm>
    {
        #region Tests
        IRenderedComponent<MatchForm> queryForm = null;
        MatchSearchRequest model = new MatchSearchRequest();
        ComponentParameterCollectionBuilder<MatchForm> parameters = new ComponentParameterCollectionBuilder<MatchForm>();

        public MatchFormTests()
        {
            InitialValues.Query = model;
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
            queryForm.Find("#Query_MatchId").Change("1234567");

            // Assert
            Assert.Equal("1234567", model.MatchId);
        }

        /// <summary>
        /// Verify that when there are no results found, an informational box pops up
        /// </summary>
        [Fact]
        public void Verify_No_Results_Alert_Box_No_Results()
        {
            // Arrange

            // Add a result with no matches
            InitialValues.QueryResult = new()
            {
                Data = new()
            };
            CreateTestComponent();

            // Assert
            var alertBox = queryForm.FindComponent<UsaAlertBox>();
            Assert.Contains("This Match ID does not have a matching record in any other states.", alertBox.Markup);
            Assert.False(queryForm.HasComponent<MatchResults>());
        }

        /// <summary>
        /// Verify that when there are results from the query, the results area is shown
        /// </summary>
        [Fact]
        public void Verify_Results_Shown()
        {
            // Arrange

            // Add a result with no matches
            InitialValues.QueryResult = new()
            {
                Data = new List<MatchResApiResponse>()
                {
                    new MatchResApiResponse()
                    {
                        Data = new MatchResRecord
                        {
                            States = new[] { "ea", "eb" }
                        }
                    }
                }
            };
            CreateTestComponent();

            // Assert
            Assert.True(queryForm.HasComponent<MatchResults>());
            Assert.False(queryForm.HasComponent<UsaAlertBox>());
        }

        /// <summary>
        /// Verify that when a valid search is performed, the submit button's text changes to "Finding Match Record..."
        /// </summary>
        [Fact]
        public async Task Button_Should_Say_Finding_Match_Record_While_Searching_Valid_Form()
        {
            // Arrange
            CreateTestComponent();
            var searchButton = queryForm.Find("#match-form-search-btn");
            var form = queryForm.FindComponent<UsaForm>();

            // Assert
            Assert.Equal("Find Match Record", searchButton.TextContent);
            Assert.False(searchButton.HasAttribute("disabled"));

            // Act
            queryForm.Find("#Query_MatchId").Change("1234567");
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
            Assert.Equal("Finding Match Record...", searchButton.TextContent);
            Assert.True(searchButton.HasAttribute("disabled"));
        }

        /// <summary>
        /// Verify that the button does not change to say "Finding Match Record..." when the form is invalid
        /// </summary>
        [Fact]
        public async Task Button_Should_Not_Say_Searching_While_Searching_Invalid_Form()
        {
            // Arrange
            CreateTestComponent();
            var searchButton = queryForm.Find("#match-form-search-btn");
            var form = queryForm.FindComponent<UsaForm>();

            // Assert
            Assert.Equal("Find Match Record", searchButton.TextContent);
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
            Assert.Equal("Find Match Record", searchButton.TextContent);
            Assert.False(searchButton.HasAttribute("disabled"));
        }

        [Fact]
        public void Form_With_Server_Error_Should_Show_Errors()
        {
            // Arrange
            InitialValues.ServerErrors = new List<ServerError> { new("Query.MatchId", $"{ValidationFieldPlaceholder} is required") };
            CreateTestComponent();

            var alertBox = queryForm.FindComponent<UsaAlertBox>();
            var alertBoxErrors = alertBox.FindAll("li");

            // Assert
            Assert.Equal(1, alertBoxErrors.Count);

            List<string> errors = new List<string>
            {
                "Match ID is required"
            };

            for (int i = 0; i < alertBoxErrors.Count; i++)
            {
                string error = alertBoxErrors[i].TextContent.Replace("\n", "");
                error = Regex.Replace(error, @"\s+", " ");
                Assert.Contains(errors[i], error);
            }
        }

        [Fact]
        public async Task Form_Should_Show_Required_Errors()
        {
            // Arrange
            CreateTestComponent();
            var form = queryForm.FindComponent<UsaForm>();

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
            var alertBox = queryForm.FindComponent<UsaAlertBox>();
            var alertBoxErrors = alertBox.FindAll("li");
            var inputErrorMessages = form.FindAll($".{InputErrorMessageClass}");

            // Assert
            Assert.False(isFormValid);
            Assert.Equal(1, alertBoxErrors.Count);
            Assert.Equal(1, inputErrorMessages.Count);

            List<string> errors = new List<string>
            {
                "Match ID is required"
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

        /// <summary>
        /// Verify that if we have results that has a vulnerable individual, clicking it shows the vulnerable match modal
        /// </summary>
        [Fact]
        public void Verify_VulnerableModal_ShownWhenClicked_VulnerableMatch()
        {
            // Arrange

            // Add a result with no matches
            InitialValues.QueryResult = new()
            {
                Data = new List<MatchResApiResponse>()
                {
                    new MatchResApiResponse()
                    {
                        Data = new MatchResRecord
                        {
                            States = new[] { "ea", "eb" },
                            Dispositions = new[]
                            {
                                new Disposition
                                {
                                    State = "ea",
                                    VulnerableIndividual = true
                                },
                                new Disposition
                                {
                                    State = "eb",
                                    VulnerableIndividual = false
                                }
                            }
                        }
                    }
                }
            };
            CreateTestComponent();

            var queryResults = queryForm.FindComponent<MatchResults>();
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
            InitialValues.QueryResult = new()
            {
                Data = new List<MatchResApiResponse>()
                {
                    new MatchResApiResponse()
                    {
                        Data = new MatchResRecord
                        {
                            States = new[] { "ea", "eb" },
                            Dispositions = new[]
                            {
                                new Disposition
                                {
                                    State = "ea",
                                    VulnerableIndividual = false
                                },
                                new Disposition
                                {
                                    State = "eb",
                                    VulnerableIndividual = false
                                }
                            }
                        }
                    }
                }
            };
            CreateTestComponent();

            var queryResults = queryForm.FindComponent<MatchResults>();
            queryResults.Find("a").Click();

            // Assert
            var modalManager = Services.GetService<IModalManager>();
            Assert.Empty(modalManager.OpenModals);
        }

        #endregion Tests

        #region Helper Function

        /// <summary>
        /// Setup the component and register Javascript mocks
        /// </summary>
        protected override void CreateTestComponent()
        {
            JSInterop.SetupVoid("piipan.utilities.registerFormValidation", _ => true).SetVoidResult();
            JSInterop.Setup<int>("piipan.utilities.getCursorPosition", _ => true).SetResult(1);
            JSInterop.SetupVoid("piipan.utilities.setCursorPosition", _ => true).SetVoidResult();
            JSInterop.SetupVoid("piipan.utilities.scrollToElement", _ => true).SetVoidResult();
            var componentFragment = RenderComponent<MatchForm>((builder) =>
            {
                builder.Add(n => n.Query, InitialValues.Query);
                builder.Add(n => n.QueryResult, InitialValues.QueryResult);
                builder.Add(n => n.ServerErrors, InitialValues.ServerErrors);
                builder.Add(n => n.Token, InitialValues.Token);
            });
            queryForm = componentFragment;
        }


        #endregion Helper Functions
    }
}
