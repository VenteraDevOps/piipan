# 35. Testing Strategy

Date: 2022-08-29

## Status
 
Proposed
 
## Context

We already employ a testing strategy that incorporates test-driven development (TDD) for developers for all .Net code, javascript, and recently for cshtml as well. These tests include both unit and integration tests. We enforce TDD by requiring that dvelopers write unit tests before writing code to satisfy JIRA tickets. All code changes are required to be reviewed via a Pull Request (PR) in GitHub and are automatically tested by our continuous integration pipelines. A PR will be blocked from merging if any tests fail. We additionally enforce automatic checks within GitHub that all PRs must have greater than 90% code coverage. By following these guidelines, we enforce loosely couple code that's easier to maintain, reduce the probability of bugs in our code prior to release, and reduce expensive rework by fixing issues earlier in the software development lifecycle.

Our currently level of code coverage is actually 95%. While this is higher than the industry standard, we still have plenty of room for improvement to our testing strategy. We want to establish a framework for incorporating a variety of other types of functional & non-functional tests (see test categories below) into our continuous integration pipelines

<ins>Functional Tests</ins>
* Smoke Tests (general quick sanity tests. Check core functionalities of code)
* Regression Tests (more detailed and extensive verification and quality checks of system as a whole. Ensures that a modification to one area does not impact other areas)
* API Tests
* End-to-end integration/system tests

<ins>Non-Functional Tests</ins>
* Security Tests 
* Accessibility tests (508 tests utilizing pa11y-ci) 
* Performance Tests 
* Concurrency Tests 
* Chaos Tests (e.g. reliability, scalability, recoverability tests)

## Decision

We will continue employing TDD practices for all code changes by writing unit tests before writing code to satisfy requirements in the JIRA ticket(s). We will continue to implement automatic CI checks that require PRs provide at least 90% coverage before they can merge into the development branch. This process involves writing a test(s) before writing code to address requirements, verifying the test(s) fails, then writing code that fulfills the requirements and allows the test(s) to pass, refactor and clean up code, verify test(s) still passes, generate a PR for code review and to merge into the development branch.

Additionally, we will take the following steps over the next year to enhance our testing

### Sept-Dec 2022
* Currently we automatically build and run tests via CircleCI integration to GitHub. We will switch over to GitHub Enterprise and utilize GitHub Actions for running CICD pipelines.
* Configure & document environment and run frequency for different categories of tests. e.g. Unit & integration tests will run against "Dev", "Test", and "Preprod" with every deployment, Performance tests will run against "Test" nightly, etc..
* Establish connectivity between Test Runner(s) (in GitHub) and deployed applications so that network based tests (i.e. API tests) can be performed on the Test Runner(s)
* Investigate approach to handle a test user and EAuth/PIV for end-to-end testing
* Investigate approach to handle multiple tester user roles for end-to-end testing
* Configure NetSparker to run Security Tests against our different environments
* Begin incorporating API tests (Postman or RestAssured)
* Create framework for running end-to-end tests that utilize a test user or users that have different roles
* Incorporate more end-to-end and accessibility tests (cypress tests)

### Jan-Mar 2023
* Incorporate more API tests
* Incorporate more end-to-end and accessibility tests 
* Begin incorporating regression tests & smoke tests
* Create framework for running performance tests as part of continuous integration

### Apr-Jun 2023
* Incoporate more performance tests
* Create framework for running concurrency tests as part of continuous integration
* Investigate chaos testing frameworks to verify reliability, scalability, and recoverability of the NAC

### July-Sept 2023
* Incorporate more concurrency tests
* Begin incorporating chaos tests 


## Consequences

By integrating a methodology for these tests across all our services and shifting the tests left, we will be able to greatly reduce code release delivery time, provide an enhanced end-to-end full coverage reducing maintenance time, and enhance our current capabilities for continuous integration and continuous deployment with a fully automated test suite.

*	Increased test reliability, trust, and business value
*	Ability to find bugs earlier in the development process
*	Ability to retest and validate bug fixes in a rapid amount of time
*	Complete testing coverage for functional and non-functional tasks

## Resources
* [Non-functional Testing](https://www.perfecto.io/blog/what-is-non-functional-testing#:~:text=The%20difference%20between%20functional%20and%20non%2Dfunctional%20testing%20is%20what,the%20functionality%20of%20an%20app)
