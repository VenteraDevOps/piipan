// An example of executing some actions before Pa11y runs.
// This example logs in to a fictional site then waits
// until the account page has loaded before running Pa11y
// pa11cy example?: https://github.com/dominicfraser/Pa11yCIExamples/blob/master/.pa11yci.json
'use strict';

const pa11y = require('pa11y');

runExample();

async function allErrors() {
	var screenCapture = `C:/Users/dborgen/Pictures/all-errors.png`;
	var actions = [
		'click element .usa-button[type="submit"]'
	];
	return runTest(screenCapture, actions);
}

async function allErrorsButLastName() {
	var screenCapture = `C:/Users/dborgen/Pictures/all-errors-but-last-name.png`;
	var actions = [
		'set field #Query_LastName to exampleLastName',
		'check field #Query_LastName', // This fires the onchange event. https://github.com/pa11y/pa11y/issues/447 & https://github.com/pa11y/pa11y/issues/513
		'click element .usa-button[type="submit"]'
	];
	return runTest(screenCapture, actions);
}

async function runTest(screenCapture, actions) {
	return pa11y('https://localhost:5001/', {
		screenCapture: screenCapture,
		// Run some actions before the tests
		actions: actions,

		// Log what's happening to the console
		log: {
			debug: console.log,
			error: console.error,
			info: console.log
		}

	});
}

// Async function required for us to use await
async function runExample() {
	try {
		const results = await Promise.all([
			allErrors(),
			allErrorsButLastName()
		]);
		for (let result of results) {
			console.log(result);
        }
	} catch (error) {

		// Output an error if it occurred
		console.error(error.message);

	}
}