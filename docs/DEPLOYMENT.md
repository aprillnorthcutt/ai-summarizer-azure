# 🚀 Deployment Guide

This project uses **Terraform** to provision Azure infrastructure for the AI-Powered Document Summarizer. It supports both:

- 💻 **Local deployment** from your machine directly to Azure
- ☁️ **Automated CI/CD** with GitHub Actions

Whether you're exploring Infrastructure as Code for the first time or setting up reproducible cloud environments, this guide walks you through everything you need to get started.

---

### 🔧 What Terraform Does

Terraform enables declarative infrastructure as code. In this project, it provisions:

- Azure Resource Group
- App Service Plan / Function App
- Azure Cognitive Services (or Azure OpenAI)
- Networking and identity resources (as needed)

Everything is deployed from code under `infra/`, organized by environment (`dev`, `test`, etc.).

---

## 💻 Local Deployment (via Azure CLI)

### ✅ 1. Prerequisites

| Tool         | Purpose                    | Install Link |
|--------------|----------------------------|--------------|
| Azure CLI    | Authenticate to Azure      | [Install](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) |
| Terraform    | Deploy infrastructure code | [Install](https://developer.hashicorp.com/terraform/downloads) |

Log in with:

```bash
az login
```

---

### 📋 2. Get Required Azure Info

You’ll need your Azure **Subscription ID** and **Tenant ID**:

```bash
az account show --query id -o tsv         # Subscription ID
az account show --query tenantId -o tsv   # Tenant ID
```

---

### 🌱 3. Set Environment Variables

Terraform expects those values as environment variables. Run:

```bash
export TF_VAR_subscription_id=$(az account show --query id -o tsv)
export TF_VAR_tenant_id=$(az account show --query tenantId -o tsv)
```

✅ You must do this every time you open a new terminal unless you use a helper script like:

```bash
source infra/envs/dev/set-env.sh
```

---

### 🚀 4. Deploy the Infrastructure

From the `dev` environment folder:

```bash
cd infra/envs/dev

terraform init                          # Set up Terraform
terraform plan -var-file="dev.tfvars"   # Preview changes
terraform apply -var-file="dev.tfvars"  # Deploy to Azure
```

---

### 🧯 Troubleshooting (Local)

| Problem                               | Solution |
|---------------------------------------|----------|
| `subscription_id` or `tenant_id` not set | You forgot to export `TF_VAR_...` variables |
| `az` not found                        | Install Azure CLI |
| Terraform plan fails after restart    | Re-run `az login` and re-export TF_VARs |

---

## ☁️ GitHub Actions Deployment (CI/CD)

This project includes a GitHub Actions pipeline to **automatically validate and plan Terraform changes** on push or pull request.

📄 Workflow: `.github/workflows/ci.yml`

---

### 🔐 1. Set Up GitHub Secrets

Go to:  
**Repo → Settings → Secrets and variables → Actions → Secrets**

Add the following:

| Secret Name             | Description                          |
|-------------------------|--------------------------------------|
| `ARM_CLIENT_ID`         | Azure service principal ID           |
| `ARM_CLIENT_SECRET`     | Azure service principal password     |
| `ARM_SUBSCRIPTION_ID`   | Your Azure subscription ID           |
| `ARM_TENANT_ID`         | Your Azure tenant ID                 |

These map to Terraform variables automatically using:

```yaml
TF_VAR_subscription_id: ${{ secrets.ARM_SUBSCRIPTION_ID }}
TF_VAR_tenant_id: ${{ secrets.ARM_TENANT_ID }}
```

---

### 🚀 2. Trigger the CI/CD Pipeline

Push a feature branch or open a pull request:

```bash
git checkout -b feature/infra-update
git commit -am "Update Terraform config"
git push origin feature/infra-update
```

GitHub will automatically:

✅ Lint and format `.tf` files  
✅ Run `terraform init`, `validate`, and `plan`  
✅ Optionally deploy (if `apply` step is enabled)

You can view progress in the **Actions** tab on GitHub.

---

## 📁 Folder Structure

```
infra/
├── envs/
│   └── dev/
│       ├── main.tf
│       ├── variables.tf
│       ├── dev.tfvars
│       ├── set-env.sh          # Optional helper script
```

Each environment (dev, test, prod) can have its own Terraform config, keeping deployments isolated and repeatable.

---

## ✅ Summary

| Task                         | Local CLI                        | GitHub Actions CI/CD         |
|------------------------------|----------------------------------|------------------------------|
| Authentication               | `az login` + env vars            | GitHub secrets               |
| Set subscription + tenant    | `TF_VAR_...` manually            | Mapped from secrets          |
| Plan infrastructure changes  | `terraform plan`                 | Auto-run on push             |
| Apply/deploy infrastructure  | `terraform apply`                | (optional via CI/CD)         |

---

> 💡 This setup makes it easy to test changes locally and safely promote them via GitHub pull requests. It's CI/CD for infrastructure — fully automated, auditable, and reproducible.
