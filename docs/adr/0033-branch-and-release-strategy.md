# 33. Branching and Release Strategy

Date: 2022-08-26

## Status
 
Proposed

## Context

As we officially begin supporting production releases in addition to supporting multiple environments (dev, test, pre-prod, etc.), the likelihood of supporting multiple versions simultaneously is increasing. We have started experiencing merging headaches with our current strategy when Dev, Main, and Preprod all had different versions. Introducing a hotfix involved backing changes out, applying fixes, re-applying backed out changes, modifying old release notes and changelogs, and fixing tags. It's becoming more important to have a solid branching and release strategy in place to support development and minimize overhead.

## Decision

As we strive to move towards a CICD workflow, we will be adopting a modified version of the Trunk-based development workflow. This workflow has increased in popularity due to it support of modern DevOps practices and CICD. Some other options considered include GitFlow, GitHub Flow, & GitLab Flow. 

We will utilize our "Dev" branch as our "trunk" and source of truth. We will strive to keep it releasable at all times. Unlike traditional trunk-based development, we will still require developers to utilize short-lived feature branches and create pull requests for review prior to merging in work. When the Git repository is no longer public, we may revisit this requirement.

We will also make use of tags for tracking milestones. We will create a "Sprint" tag for the end of every sprint and we will create a "Release" tag for every potential release candidate. 

We will also keep "Environment" tags. Each environment will have a moving tag pointing to its current state, this will allow to know instantly which version is deployed in each environment. Upon deploying to an environment, we will move that enviornment's tag to the appropriate commit that the release was generated from.

We will deploy from the "Dev" branch to the development environment with every commit. We will also deploy from the "Dev" branch to test and preprod environments based on the desired "Release" tag. 

If fixes or changes are necessary for a release, we will generate a release branch from the "Release" tag, apply fixes/changes, deploy the updates, and merge the changes back to the "Dev" branch. It's important to note, that once a release branch has been generated and used to apply a fix/change, this branch should stay in existence until a new version is deployed to production. It can be deleted once the version no longer exists in any environment.

## Consequences

This approach should reduce merge headaches as we no longer have to maintain a separate "Main" branch. This branch was intended to provide a stable means of deployment while allowing developers to continue working on the "Dev" branch. However, this same objective can be achieved with the use of tags as described above. Developers continue working and updating the "Dev" branch, and releases (and release branches when necessary) can be generated from any tag.

By not having to merge into "Main" each sprint, we should never need to revert Sprint work.

When hotfixes are necessary, a release branch will be generated from the "production" environment tag as mentioned above. This workflow supports making the fixes, applying them, and merging them back to "Dev". 

## Resources
* [Branching Strategies](https://www.flagship.io/git-branching-strategies/)
* [Git Flow](https://www.atlassian.com/git/tutorials/comparing-workflows/gitflow-workflow)
* [GitLab Flow](https://docs.gitlab.com/ee/topics/gitlab_flow.html)
* [Trunk-based development vs Git Flow](https://www.toptal.com/software/trunk-based-development-git-flow)
