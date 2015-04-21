Authors: Logan Gore, Christopher Weeter

We ran into several issues dealing with opening multiple clients, but fixed our server to resolve them

Had an issue with sharing the client model among the pages but found the Application.Current.Properties

Used Application.Current.Properties to share client model

With the client model we were able to store all the necessary data for all of the UI to use

The ui simply implemented event handlers and used the information from the handlers to conduct business

When users want to replay, we resolved to sending them back to the connection page with their information
preserved into the text fields