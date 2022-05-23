let pa11yOptions = {};

describe('query tool match query', () => {
    beforeEach(() => {
        pa11yOptions = {
            actions: [
                'wait for element h1 to be added'
            ],
            standard: 'WCAG2AA',
            runners: [
                'htmlcs'
            ],
        };
        cy.visit('/list');
        cy.get('h1', { timeout: 10000 }).should('be.visible');
    })

    it("shows results table", () => {
        cy.contains('Match ID').should('be.visible');
        cy.contains('Matching States').should('be.visible');

        cy.pa11y(pa11yOptions);
    });
})