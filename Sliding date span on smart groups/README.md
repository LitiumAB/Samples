# Sliding date span on smart groups

**Tested in Litium version: 7.2.2**

When creating a smart group in the customer area it is possible to add a condition that checks if order total meet a set condition (for example order total is above a given amount) in a set date interval.

The problem is that often you want that date interval to slide, so that you for example can check order total in the last 2 weeks and not define a specific start and end date.

The task in the resources folder solves this by adjusting the set date interval every time the task is executed, just schedule it to run every night to automatically update the date interval.

## Instructions

1. Create a integer field called **SlidingDateSpanDays** and add it to a group template
2. Create a smart group with the template and set a value in the **SlidingDateSpanDays**-field
3. Add the Order total condition to your smart group with any date interval
4. Schedule the task in _Web.config_ with: `<scheduledTask type="Litium.Accelerator.Tasks.UpdateDatespanOnGroupsTask, Litium.Accelerator" startTime="00:30" interval="1d" />`
5. When the task has executed verify that the date interval on the group is set to start the set number of days back and end the day after today (the task finds and updates all groups that have a value set in the **SlidingDateSpanDays**-field)
