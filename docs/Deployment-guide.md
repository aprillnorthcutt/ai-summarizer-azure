# 🧠 AI-Powered Document Summarizer – Infrastructure Deployment Guide

This project provisions Azure infrastructure for the **AI-Powered Document Summarizer** using **Terraform**, a secure **service principal**, and **CI/CD workflows via GitHub Actions**.

You can deploy infrastructure using either:

- 💻 **Local CLI** with Azure authentication  
- ☁️ **GitHub Actions** using securely stored secrets and automated pipelines  

Whether you're just starting with Terraform or scaling enterprise DevOps, this guide shows how to get started and grow with confidence.

---

## 🧱 What Terraform Provisions

Terraform modules under `/infra/` provision:

- 🔹 Azure Resource Group  
- 🔹 App Service Plan (Linux, cost-optimized)  
- 🔹 Azure App Service (for .NET Web App)  
- 🔹 Azure Cognitive Services (or Azure OpenAI)  
- 🔹 Optional: Networking, identity, tagging (e.g., `environment = dev`)  
- 🔹 Optional: Remote state backend (via `backend.tf`)

---

## 💻 Local Deployment (Terraform CLI)

### ✅ 1. Prerequisites

| Tool         | Purpose                   | Install Link |
|--------------|---------------------------|--------------|
| Azure CLI    | Authenticate to Azure     | [Install](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) |
| Terraform    | Deploy infrastructure     | [Install](https://developer.hashicorp.com/terraform/downloads)          |

Login to Azure:

```bash
az login
```

---

### 📋 2. Get Required Azure Info

```bash
az account show --query id -o tsv         # Subscription ID
az account show --query tenantId -o tsv   # Tenant ID
```

---

### 🔐 3. Set Environment Variables

You’ll need your Azure subscription, tenant ID, and service principal credentials:

```bash
export TF_VAR_subscription_id="<your-subscription-id>"
export TF_VAR_tenant_id="<your-tenant-id>"
export TF_VAR_client_id="<your-app-client-id>"
export TF_VAR_client_secret="<your-app-client-secret>"
```

Or use the helper script:

```bash
source infra/envs/dev/set-env.sh
```

---

### 🚀 4. Deploy the Infrastructure

```bash
cd infra/envs/dev
terraform init
terraform plan -var-file="dev.tfvars"
terraform apply -var-file="dev.tfvars"
```

---

## ☁️ GitHub Actions Deployment (CI/CD)

This project uses **two GitHub Actions workflows**:

- 📄 `.github/workflows/ci-terraform.yml` – Validates and plans Terraform infrastructure
- 📄 `.github/workflows/ci-dotnet.yml` – Builds and deploys the .NET web application

---

### 🔐 1. Set GitHub Secrets

Go to:  
**Repo → Settings → Secrets and variables → Actions → Secrets**

| Secret Name             | Description                          |
|-------------------------|--------------------------------------|
| `ARM_CLIENT_ID`         | Azure service principal ID           |
| `ARM_CLIENT_SECRET`     | Azure service principal password     |
| `ARM_SUBSCRIPTION_ID`   | Azure subscription ID                |
| `ARM_TENANT_ID`         | Azure tenant ID                      |

These secrets map into Terraform variables automatically.

---

### 🚀 2. Trigger Terraform CI/CD

```bash
git checkout -b feature/infra-update
git commit -am "Update Terraform config"
git push origin feature/infra-update
```

`ci-terraform.yml` will:

- ✅ Lint and validate `.tf` files  
- ✅ Run `terraform init`, `validate`, and `plan`  
- ✅ (Optional) Run `apply` if configured

---

### 🚀 3. Trigger .NET App CI/CD

```bash
git checkout -b feature/app-deploy
git commit -am "Update .NET web app"
git push origin feature/app-deploy
```

`ci-dotnet.yml` will:

- ✅ Build and test the .NET app  
- ✅ Deploy to Azure App Service

---

## 🔍 CI/CD Highlights

| Task                         | Local CLI             | GitHub Actions CI/CD         |
|------------------------------|-----------------------|------------------------------|
| Authentication               | `az login` + env vars | GitHub secrets               |
| Set subscription + tenant    | `TF_VAR_...` manually | Auto-mapped from secrets     |
| Plan infrastructure changes  | `terraform plan`      | Auto-run on push             |
| Apply infrastructure         | `terraform apply`     | Optional (safe by default)   |
| Deploy app code              | Manual/CLI            | Auto-run via `ci-dotnet.yml` |

> ⚠️ Infrastructure CI runs `terraform plan` only by default — to avoid unintentional costs.

---

## 🧯 Troubleshooting

| Problem                               | Solution |
|---------------------------------------|----------|
| `subscription_id` or `tenant_id` not set | Ensure you exported `TF_VAR_` env vars |
| `az` not found                        | Install Azure CLI |
| Terraform fails after restart         | Re-run `az login` and re-export env vars |
| CI/CD job fails                       | Check GitHub secrets or missing tfvars |

---

## 🧪 Dev/Test/Prod Environment Support

Terraform environments are separated under `/infra/envs/`, each with its own configuration:

```
infra/
├── envs/
│   ├── dev/
│   │   ├── main.tf
│   │   ├── variables.tf
│   │   ├── dev.tfvars
│   │   └── set-env.sh
│   ├── test/
│   └── prod/
```

This supports isolated deployments, consistent tagging, and safer staged rollouts.

---

## ✅ Summary

- ✅ Supports **manual CLI** and **automated GitHub Actions** infrastructure deployment
- ✅ Uses **Azure service principal** for secure non-interactive authentication
- ✅ CI/CD is split by concern: infra (`ci-terraform.yml`) vs app (`ci-dotnet.yml`)
- ✅ Designed for multi-environment pipelines: **dev**, **test**, **prod**

> 💡 Test infrastructure locally, promote via pull requests, and deploy with confidence. This setup is safe, scalable, and DevOps-ready.


---

## 🔄 Coming Soon: Full DevOps Pipeline

This project will soon include a robust DevOps pipeline integrating:

- ✅ Infrastructure provisioning via Terraform
- ✅ Application build and deploy via GitHub Actions
- 🔜 **CI/CD pipeline integration with Azure DevOps (YAML-based)**
- 🔜 **Multi-stage release pipeline with approvals**
- 🔜 **Monitoring and observability using Application Insights**
- 🔜 **Automated rollback on deployment failure**
- 🔜 **Key Vault integration for secret management**

Stay tuned! 🚀

---

## 📁 Planned DevOps Folder Structure

```
.devops/
├── pipelines/
│   ├── build-app.yml
│   ├── release-app.yml
│   └── terraform-deploy.yml
├── templates/
│   └── azure-app-service-template.json
```
