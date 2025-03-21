## Features
- Orchestrates activities through [Dapr Workflow](https://docs.dapr.io/developing-applications/building-blocks/workflow/workflow-overview/).
- Executes one external job to demonstrate external workloads (locally on Docker, but in production with K8S on specific job).

## Usage
1. Create a Docker network:
   ```bash
   docker network create workflow-network
   ```
2. Bring up the containers:
   ```bash
   docker compose up -d
   ```
3. View logs:
   ```bash
   docker compose logs -f
   ```

## Explaination

The idea is to use Workflow to orchestrate some external k8s job in order to process compute in a cost-optimized way. Let say the job is running under GPU we can spawn k8s job on these particular node for compute time only and release the node after the job done by leverage k8s autoscalling.

There is a quick sequence diagram of the actual POC :

```mermaid
sequenceDiagram
	User->>App: Can you please process my message.
	App->>Workflow: Start processing this message.
	Workflow->>CamelCaseActivity: Process this message.
    CamelCaseActivity->>Workflow: There is the result.
	Workflow->>JobOrchestrator: Can you submit a job to process this message ?
	 alt OK
        JobOrchestrator->>Job: Pop a job.
        JobOrchestrator->>Workflow: Your message will be processed by Job with Id=1.
	  else KO
	      loop Retry Loop
	          JobOrchestrator->>Workflow: Can you retry please ?
	          Workflow->>JobOrchestrator: Can you process this message please ?
	      end
	  end
	Workflow->>Workflow: Waiting for ExternalEvent with JobId=1
    Job->>App: I am Job with Id=1 you can tell the Workflow with Id=1 to continue.
	App->>Workflow: You have an ExternalEvent JobId=1. 
	Workflow->>App: The message has been processed.
	App->>User: There is your processed message.
```

We are using ID as `ExternalEvent` to ensure uniqueness of a job, we can easily imagine launching N job at the same time to parallelise the work and wait for all of them to be completed before resuming the workflow.