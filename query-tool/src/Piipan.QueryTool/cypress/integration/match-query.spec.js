const pa11yOptions = {
    hideElements: '.cui, [aria-controls="nav-user"]'
};

describe('query tool match query', () => {
  beforeEach(() => {
    cy.visit('https://localhost:5001');
  })

  it('shows required field errors when form is submitted with no data', () => {
    cy.get('form').submit();

    cy.contains('Last Name is required').should('be.visible');
    cy.contains('Date of Birth is required').should('be.visible');
      cy.contains('Social Security Number is required').should('be.visible');
      cy.pa11y(pa11yOptions);
  });

  it("shows formatting error for incorrect SSN", () => {
      cy.get('#Query_SocialSecurityNum').type("12345");
      cy.pause().debug();
      cy.get('form').submit();

      cy.contains('Social Security Number must have the form ###-##-####').should('be.visible');
      cy.pa11y(pa11yOptions);
  });

  //it("shows proper error for too old dates of birth", () => {
  //  cy.get('#Query_DateOfBirth').type("1899-12-31");
  //  cy.get('form').submit();

  //  cy.contains('Date of birth must be between 01-01-1900 and today\'s date').should('be.visible');
  //});

  //it("shows proper error for non-ascii characters in last name", () => {
  //  cy.get('input[name="Query.LastName"]').type("garcía");
  //  // Enter other valid form inputs to isolate expected error
  //  cy.get('input[name="Query.FirstName"]').type("joe");
  //  cy.get('input[name="Query.DateOfBirth"]').type("1997-01-01");
  //  cy.get('input[name="Query.SocialSecurityNum"]').type("550-01-6981");

  //  cy.get('form').submit();

  //  cy.contains('Change í in garcía').should('be.visible');
  //});

  // it("shows an empty state on successful submission without match", () => {


  //   cy.get('input[name="Query.FirstName"]').type("joe");
  //   cy.get('input[name="Query.LastName"]').type("schmo");
  //   cy.get('input[name="Query.DateOfBirth"]').type("1997-01-01");
  //   cy.get('input[name="Query.SocialSecurityNum"]').type("550-01-6981");

  //   cy.get('form').submit();

  //   cy.contains('No matches found').should('be.visible');
  // });

  // it("shows results table on successful submission with a match", () => {
  //   // TODO: stub out submit request
  //   cy.get('input[name="Query.FirstName"]').type("Theodore");
  //   cy.get('input[name="Query.LastName"]').type("Farrington");
  //   cy.get('input[name="Query.DateOfBirth"]').type("1931-10-13");
  //   cy.get('input[name="Query.SocialSecurityNum"]').type("425-46-5417");

  //   cy.get('form').submit();

  //   cy.contains('Results').should('be.visible');
  //   cy.contains('Case ID').should('be.visible');
  //   cy.contains('Participant ID').should('be.visible');
  //   cy.contains('Benefits end month').should('be.visible');
  //   cy.contains('Recent benefit months').should('be.visible');
  //   cy.contains('Protect location?').should('be.visible');
  // });
})
