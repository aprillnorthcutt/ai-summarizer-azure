🚀 Deployment Guide (Terraform + Azure + GitHub Actions)

This project supports deploying Azure infrastructure for the AI-Powered Document Summarizer using modular Terraform, a service principal, and CI/CD workflows.

You can deploy infrastructure via:

💻 Local CLI with Azure login and environment variables

☁️ GitHub Actions using securely stored secrets and pipelines

Whether you're new to Terraform or building enterprise-grade DevOps, this guide shows how to get started and scale.

🧱 What Infrastructure is Provisioned

Terraform modules under /infra/ provision:

Azure Resource Group

App Service Plan (Linux, cost-optimized)

Azure App Service for .NET

Azure Cognitive Services (or OpenAI)

Optional: Networking, identity, and tagging (e.g., environment = dev)

💻 Local Deployment (Terraform CLI)

✅ Prerequisites

Tool

Purpose

Install Link

Azure CLI

Authenticate to Azure

Install

Terraform

Deploy infrastructure code

Install

Run login:

az login

🔐 Set Required Environment Variables

You’ll need your Azure Subscription ID, Tenant ID, and Service Principal credentials.

export TF_VAR_subscription_id="<your-subscription-id>"
export TF_VAR_tenant_id="<your-tenant-id>"
export TF_VAR_client_id="<your-app-client-id>"
export TF_VAR_client_secret="<your-app-client-secret>"

✅ Or use a helper script:

source infra/envs/dev/set-env.sh

🚀 Run Terraform

From the dev environment folder:

cd infra/envs/dev
terraform init
terraform plan -var-file="dev.tfvars"
terraform apply -var-file="dev.tfvars"

☁️ GitHub Actions Deployment (CI/CD)

This project includes a GitHub Actions workflow that automatically validates and plans Terraform code on every push or PR.

📄 Workflow file: .github/workflows/ci.yml

🔐 Set GitHub Secrets

Go to your GitHub repo:
Settings → Secrets and variables → Actions → Secrets

Add these secrets:

Secret Name

Description

ARM_CLIENT_ID

Azure service principal ID

ARM_CLIENT_SECRET

Azure service principal password

ARM_SUBSCRIPTION_ID

Your Azure subscription ID

ARM_TENANT_ID

Your Azure tenant ID

These secrets are automatically mapped into Terraform variables by the GitHub Actions environment.

✅ Trigger CI/CD

To run the workflow:

git checkout -b feature/infra-update
git commit -am "Update Terraform config"
git push origin feature/infra-update

GitHub Actions will:

✅ Lint and validate .tf files

✅ Run terraform init, validate, and plan

✅ (Optional) Run apply if automation is enabled

You can monitor progress under the Actions tab in GitHub.

🔍 CI/CD Highlights

Stage

Local CLI

GitHub Actions (CI/CD)

Auth

az login

GitHub Secrets (SP creds)

Set Environment Vars

export TF_VAR_...

Auto-injected from secrets

Plan Infra Changes

terraform plan

Auto-runs on PR or push

Apply Infra

terraform apply

Optional (manual/auto)

⚠️ CI/CD currently performs terraform plan, not apply, by default. This is intentional for safety and cost control.

🧯 Troubleshooting

Problem

Solution

subscription_id or tenant_id missing

Export TF_VAR env vars correctly

az not found

Install Azure CLI

Plan fails after restart

Re-run az login + reset variables

CI/CD job fails

Check GitHub secrets or missing tfvars

🧪 Dev/Test/Prod Support

Each environment (dev, test, prod) is structured as a separate folder under /infra/envs/, with its own:

main.tf

variables.tf

terraform.tfvars

Optional: backend.tf for remote state

This enables environment isolation, consistent tagging, and staged rollouts.

infra/
├── envs/
│   ├── dev/
│   ├── test/
│   └── prod/

✅ Summary

Supports manual CLI and automated CI/CD Terraform deployment

Uses Azure service principal for secure non-interactive auth

Leverages GitHub Actions for repeatable, auditable workflows

Designed for easy expansion to multiple environments and full pipelines

💡 This setup makes it easy to test infra locally and promote changes via pull requests with confidence — cloud deployments made safe, scalable, and DevOps-ready.