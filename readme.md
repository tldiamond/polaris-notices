#Getting Started
##What is this thing?
This is an application to enable better notice handling for the Polaris ILS, plus a couple miscellaneous extras. It lets you automate the handling of bounced email messages and spam reports. When one of these events is triggered the application will take the following steps:
1. Add all patron records using that email address to a record set, based upon the **Parent Organization** of that patron's registered library
2. Change their delivery option from email to phone, if they have one, or to mail otherwise
3. Remove the email address from their account, from either the regular or alternate email address field
4. Add a non-blocking note to their account with the date, the old email address, and a brief description of why the address was removed.

##Mandrill Setup
###Email Bounces & Spam Reports
* Set up a webhook to trigger on hard_bounce, soft_bounce, spam, unsub, and reject that posts to `https://[your url]/email/processfailure?auth=[your auth string]`

###Sending SMS messages
* Set up a webhook to trigger on inbound messages for your chosen sms domain that posts to `https://[your url]/sms/process?auth=[your auth string]`

###Example Mandril Setup
![Example Mandrill Setup](http://media.virbcdn.com/files/77/aceb3fd114e25e11-mandrill_setup.png "Example Mandrill Setup")

##Application Configuration
* Copy the user.config.example file and rename to user.config
* Fill in the required fields in the user.config file
  * The database connections use integrated security, so you'll want to make sure that whatever account you use to run the application pool has the necessary database permissions
* Open the two resources files in /Resources and populate them with the **Parent Organization** Abbreviation and Recordset ID for each mail event
  * This is the way the application is currently set up to work, but could easily be tweaked to just dump everything into one giant recordset if desired
* Build the project to make sure that all the nuget packages are pulled in properly
* Publish and test


##Testing
* Email bounces
  * `https://[your url]/Email/TestBounce?Auth=[your auth string]&email=[your test email account] `
* Email spam reports
  * `https://[your url]/Email/TestSpam?Auth=[your auth string]&email=[your test email account] `
* Sending SMS
  * Make sure your Twilio credentials and SMS test phone number are supplied in the user.config
    * `https://[your url]/Sms/Test?auth=[your auth string] `
