using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Activities;
using System.Linq.Expressions;

namespace FINTRAC.CustomWorkflowSteps
{
    public class UpdateContactOrganization : CodeActivity
    {
        [Input("Contact")]
        [RequiredArgument]
        [ReferenceTarget("contact")]
        public InArgument<EntityReference> ContactRecord { get; set; }

        [Input("Org Number")]
        [RequiredArgument]
        public InArgument<string> OrganizationNumber { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            // Initialize services
            IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();
            IOrganizationService service = context.GetExtension<IOrganizationServiceFactory>().CreateOrganizationService(workflowContext.UserId);

            try
            {
                // Get input parameters
                EntityReference contactRef = ContactRecord.Get(context);
                string orgNumber = OrganizationNumber.Get(context);

                if (string.IsNullOrWhiteSpace(orgNumber))
                {
                    throw new InvalidPluginExecutionException("Organization Number cannot be empty.");
                }

                // Query for account with matching organization number
                QueryExpression query = new QueryExpression("account")
                {
                    ColumnSet = new ColumnSet("accountid", "name"),
                    Criteria = new FilterExpression
                    {
                        FilterOperator = LogicalOperator.And,
                        Conditions =
                        {
                            new ConditionExpression("fintrac_orgnumber", ConditionOperator.Equal, orgNumber),
                            //new ConditionExpression("statecode", ConditionOperator.Equal, 0) // Only active accounts
                        }
                    }
                };

                EntityCollection results = service.RetrieveMultiple(query);

                if (results.Entities.Count == 0)
                {
                    throw new InvalidPluginExecutionException($"No active account found with Organization Number: {orgNumber}");
                }

                if (results.Entities.Count > 1)
                {
                    throw new InvalidPluginExecutionException($"Multiple accounts found with Organization Number: {orgNumber}. Please ensure organization numbers are unique.");
                }

                // Get the matching account
                Entity account = results.Entities[0];

                // Update the contact record with the parent organization/company name
                Entity updateContact = new Entity("contact", contactRef.Id);
                updateContact["parentcustomerid"] = new EntityReference("account", account.Id);
                //updateContact["companyname"] = account.GetAttributeValue<string>("name");

                service.Update(updateContact);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException($"Error in UpdateContactOrganization workflow: {ex.Message}", ex);
            }
        }
    }


    public class UpdatePrimaryContact : CodeActivity
    {
        [Input("Organization (account)")]
        [RequiredArgument]
        [ReferenceTarget("account")]
        public InArgument<EntityReference> OrganizationRecord { get; set; }

        [Input("Contact Number")]
        [RequiredArgument]
        public InArgument<string> ContactNumber { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            // Initialize services
            IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();
            IOrganizationService service = context.GetExtension<IOrganizationServiceFactory>().CreateOrganizationService(workflowContext.UserId);

            try
            {
                // Get input parameters
                EntityReference orgRef = OrganizationRecord.Get(context);
                string contactNumber = ContactNumber.Get(context);

                if (string.IsNullOrWhiteSpace(contactNumber))
                {
                    throw new InvalidPluginExecutionException("Contact Number cannot be empty.");
                }

                // Query for account with matching organization number
                QueryExpression query = new QueryExpression("account")
                {
                    ColumnSet = new ColumnSet("primarycontactid", "name"),
                    Criteria = new FilterExpression
                    {
                        FilterOperator = LogicalOperator.And,
                        Conditions =
                        {
                            new ConditionExpression("fintrac_organizationnumber", ConditionOperator.Equal, contactNumber),
                            //new ConditionExpression("statecode", ConditionOperator.Equal, 0) // Only active accounts
                        }
                    }
                };

                EntityCollection results = service.RetrieveMultiple(query);

                if (results.Entities.Count == 0)
                {
                    throw new InvalidPluginExecutionException($"No active contact found with Contact Number: {contactNumber}");
                }

                if (results.Entities.Count > 1)
                {
                    throw new InvalidPluginExecutionException($"Multiple contacts found with Contact Number: {contactNumber}. Please ensure contact numbers are unique.");
                }

                // Get the matching account
                Entity contact = results.Entities[0];

                // Update the contact record with the parent organization/company name
                Entity updateOrg = new Entity("account", orgRef.Id);
                updateOrg["primarycontactid"] = new EntityReference("contact", contact.Id);
                //updateContact["companyname"] = account.GetAttributeValue<string>("name");

                service.Update(updateOrg);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException($"Error in UpdatePrimaryContact workflow: {ex.Message}", ex);
            }
        }
    }





}