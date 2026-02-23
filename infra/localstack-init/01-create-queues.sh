#!/bin/bash
# Creates the SQS queues in LocalStack when it starts
awslocal sqs create-queue --queue-name order-placed
awslocal sqs create-queue --queue-name stock-reserved

echo "SQS queues 'order-placed' and 'stock-reserved' created."
