> Durable function patterns

### External events

* Accept a SMS message to a customer through HTTP endpoint.
* Store this data in an Azure table storage.
* Send SMS to the customer.
* Wait for the delivery response.
* If the delivery response is successful, update the customer record stating that the message has been succesfully sent.
* Else update the customer record stating that the message could not be sent.