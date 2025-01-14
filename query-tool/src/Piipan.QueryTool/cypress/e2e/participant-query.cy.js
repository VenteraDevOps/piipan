let pa11yOptions = {};

describe('query tool match query', () => {
    beforeEach(() => {
        pa11yOptions = {
            actions: [
                'wait for element #QueryFormData_Query_SocialSecurityNum to be added'
            ],
            standard: 'WCAG2AA',
            runners: [
                'htmlcs'
            ],
        };
        cy.visit('/');
        cy.get('#query-form-search-btn', { timeout: 10000 }).should('be.visible');
    })

    it('shows required field errors when form is submitted with no data', () => {
        cy.get('form').submit();

        cy.get('#QueryFormData_Query_LastName-message').contains('Last Name is required').should('be.visible');
        cy.get('#QueryFormData_Query_DateOfBirth-message').contains('Date of Birth is required').should('be.visible');
        cy.get('#QueryFormData_Query_SocialSecurityNum-message').contains('Social Security Number is required').should('be.visible');

        // make sure pa11y runs successfully when errors are shown
        pa11yOptions.actions.push('click element #query-form-search-btn');
        cy.pa11y(pa11yOptions);
    });

    it("shows formatting error for incorrect SSN", () => {
        cy.get('#QueryFormData_Query_SocialSecurityNum').type("12345").blur();
        cy.get('#QueryFormData_Query_SocialSecurityNum-message').contains('Social Security Number must have the form ###-##-####').should('be.visible');

        cy.get('form').submit();

        cy.get('.usa-alert').contains('Social Security Number must have the form ###-##-####').should('be.visible');
    });

    it("shows proper error for too old dates of birth", () => {
        cy.get('#QueryFormData_Query_DateOfBirth').type("1899-12-31").blur();
        cy.get('#QueryFormData_Query_DateOfBirth-message').contains('Date of Birth must be between 01-01-1900 and today\'s date').should('be.visible');
        cy.get('form').submit();

        cy.get('.usa-alert').contains('Date of Birth must be between 01-01-1900 and today\'s date').should('be.visible');
    });

    it("shows proper error for non-ascii characters in last name", () => {
        cy.get('#QueryFormData_Query_LastName').type("garcía").blur();
        cy.get('#QueryFormData_Query_LastName-message').contains('Change í in garcía').should('be.visible');
        cy.get('form').submit();

        cy.get('.usa-alert').contains('Change í in garcía').should('be.visible');
    });

    // Commented out for now. We need a long-term solution for end-to-end testing before these tests will work.

    //it("shows an empty state on successful submission without match", () => {
    //    cy.get('#QueryFormData_Query_LastName').type("schmo");
    //    cy.get('#QueryFormData_Query_DateOfBirth').type("1997-01-01");
    //    cy.get('#QueryFormData_Query_SocialSecurityNum').type("550-01-6981");

    //    cy.get('form').submit();

    //    cy.contains('This participant does not have a matching record in any other states.').should('be.visible');

    //    setupPa11yPost();
    //    cy.pa11y(pa11yOptions);
    //});

    //it("shows results table on successful submission with a match", () => {
    //    setValue('#QueryFormData_Query_LastName', 'Farrington');
    //    setValue('#QueryFormData_Query_DateOfBirth', '1931-10-13');
    //    setValue('#QueryFormData_Query_SocialSecurityNum', '425-46-5417');
    //    cy.get('#query-form-search-btn').click();

    //    cy.contains('Match ID').should('be.visible');
    //    cy.contains('Matching State').should('be.visible');

    //    setupPa11yPost();
    //    cy.pa11y(pa11yOptions);
    //});
})

function setValue(cssSelector, value) {
    cy.get(cssSelector).type(value);
}
function setupPa11yPost() {
    pa11yOptions.headers = {
        'Content-Type': 'application/x-www-form-urlencoded'
    };
    pa11yOptions.method = 'POST';
    cy.get('#snap-participants-query-form input[name]').each(el => {
        const value = el.val();
        const name = el.attr('name');
        if (value && name) {
            if (pa11yOptions.postData) {
                pa11yOptions.postData += `&${name}=${value}`;
            }
            else {
                pa11yOptions.postData = `${name}=${value}`;
            }
        }
    });
}