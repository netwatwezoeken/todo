---
name: run-app
description: Always use this skill when starting the program, the app, project or when aspire needs to run or start. Examples that , start the app, start the project, run the website.
---

This project uses aspire. Use the aspire skills to run analyse and debug the project.

`todo-web-server` is the main application

In most cases you should do the following:
1. start aspire: `aspire start --isolated`
2. wait until it is running: `aspire wait todo-web-server`
3. get the webshop url: `aspire describe todo-web-server`

IMPORTANT: for browser interaction always use `playwright-cli` instead of the system default browser. When asked to do something in the app or browser, do it using `playwright-cli`. See also [playwright-cli skill](../playwright-cli/SKILL.md)