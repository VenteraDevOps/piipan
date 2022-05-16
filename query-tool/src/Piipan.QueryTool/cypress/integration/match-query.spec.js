let pa11yOptions = {};

describe('query tool match query', () => {
    beforeEach(() => {
        pa11yOptions = {
            actions: [
                'wait for element #match-form-search-btn to be added'
            ],
            standard: 'WCAG2AA',
            runners: [
                'htmlcs'
            ],
        };
        cy.visit('/match');
        cy.get('#match-form-search-btn', { timeout: 10000 }).should('be.visible');
    })

    it('shows required field errors when form is submitted with no data', () => {
        cy.get('form').submit();

        cy.get('#Query_MatchId-message').contains('Match ID is required').should('be.visible');

        // make sure pa11y runs successfully when errors are shown
        pa11yOptions.actions.push('click element #match-form-search-btn');
        cy.pa11y(pa11yOptions);
    });

    it("shows number of characters error for match ID", () => {
        cy.get('#Query_MatchId').type("12345").blur();
        cy.get('#Query_MatchId-message').contains('Match ID must be 7 characters').should('be.visible');

        cy.get('form').submit();

        cy.get('.usa-alert').contains('Match ID must be 7 characters').should('be.visible');
    });

    it("shows invalid characters error for match ID", () => {
        cy.get('#Query_MatchId').type("m12$345").blur();
        cy.get('#Query_MatchId-message').contains('Match ID contains invalid characters').should('be.visible');

        cy.get('form').submit();

        cy.get('.usa-alert').contains('Match ID contains invalid characters').should('be.visible');
    });

    it("shows an empty state on successful submission without match", () => {
        cy.get('#Query_MatchId').type("1234567").blur();

        cy.get('form').submit();

        cy.contains('This Match ID does not have a matching record in any other states.').should('be.visible');

        setupPa11yPost();
        cy.pa11y(pa11yOptions);
    });

    it("server errors are shown and accessible", () => {
        cy.get('#Query_MatchId').type("123$567").blur();

        setupPa11yPost();
        cy.pa11y(pa11yOptions);
    });

    it("shows results table on successful submission with a match", () => {
        cy.visit('/');
        cy.get('#query-form-search-btn', { timeout: 10000 }).should('be.visible');
        setValue('#Query_LastName', 'Farrington');
        setValue('#Query_DateOfBirth', '1931-10-13');
        setValue('#Query_SocialSecurityNum', '425-46-5417');
        cy.get('#query-form-search-btn').click();

        cy.get('#query-results-area tbody tr td a').invoke('text').then(matchId => {
            cy.visit('/match');
            cy.get('#match-form-search-btn', { timeout: 10000 }).should('be.visible');
            cy.get('#Query_MatchId').type(matchId).blur();
            cy.get('form').submit();

            cy.contains('Match ID').should('be.visible');
            cy.contains('Matching States').should('be.visible');

            setupPa11yPost();
            cy.pa11y(pa11yOptions);
        });
    });
})

function setValue(cssSelector, value) {
    cy.get(cssSelector).type(value);
}
function setupPa11yPost() {
    pa11yOptions.headers = {
        'Content-Type': 'application/x-www-form-urlencoded'
    };
    pa11yOptions.method = 'POST';
    cy.get('#match-form input[name]').each(el => {
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