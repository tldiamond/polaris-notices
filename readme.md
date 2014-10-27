#Getting Started
##What is this thing?
This is an application to enable better notice handling for the Polaris ILS, plus a couple miscellaneous extras. It lets you automate the handling of bounced email messages and spam reports. When one of these events is triggered the application will take the following steps:

1. Add all patron records using that email address to a record set, based upon the **Parent Organization** of that patron's registered library
2. Change their delivery option from email to phone, if they have one, or to mail otherwise
3. Remove the email address from their account, from either the regular or alternate email address field
4. Add a non-blocking note to their account with the date, the old email address, and a brief description of why the address was removed.

This application does **NOT** require the Polaris API and has been primarily used and tested against version 4.1R2 1036, with very small amounts of testing going against our 5.0 beta development server. Nothing in this application is guaranteed and it's use is solely at your own risk. I take no responsibility for anything that may happen to your database as a result of the use of this software. While setting up and configuring the system you should take caution to not point it at your production server and instead point to a well backed up training or development server instead.

##Mandrill Setup
###Email Bounces & Spam Reports
* Set up a webhook to trigger on hard_bounce, soft_bounce, spam, unsub, and reject that posts to `https://[your url]/email/processfailure?auth=[your auth string]`

###Sending SMS messages
* Set up a webhook to trigger on inbound messages for your chosen sms domain that posts to `https://[your url]/sms/process?auth=[your auth string]`

###Example Mandril Setup
![Example Mandrill Setup](http://media.virbcdn.com/files/77/aceb3fd114e25e11-mandrill_setup.png "Example Mandrill Setup")

###SMS Configuration and Other General Information
* You can find Wes's slides from PUG 2014 with more setup information at http://media.virbcdn.com/files/17/6b7afe76865df50e-CloudPUG2014.pdf

##Application Configuration
These steps are from Visual Studio 2013, depending on your version and environment some adjustments may be necessary.
* Open the solution in Visual Studio and do a build to make sure all the nuget packages get installed properly
* Create a copy of the user.config.example file and rename to user.config
* Fill in the required fields in the user.config file
  * The database connections use integrated security, so you'll want to make sure that whatever account you use to run the application pool has the necessary database permissions
* Open the two resources files in /Resources and populate them with the **Parent Organization** Abbreviation and Recordset ID for each mail event
  * This is the way the application is currently set up to work, but could easily be tweaked to just dump everything into one giant recordset if desired
* Publish the project to your IIS web server
* Configure the application pool to use Windows domain credentials with database access
* Test

##Testing
* Email bounces
  * `https://[your url]/Email/TestBounce?Auth=[your auth string]&email=[your test email account] `
* Email spam reports
  * `https://[your url]/Email/TestSpam?Auth=[your auth string]&email=[your test email account] `
* Sending SMS
  * Make sure your Twilio credentials and SMS test phone number are supplied in the user.config
    * `https://[your url]/Sms/Test?auth=[your auth string] `
