# 🛠️ Infrastructure as Code (Terraform + Azure)

This folder contains Terraform configuration to deploy the infrastructure needed for the AI-powered document summarizer. It provisions:

- Azure Resource Group
- Linux App Service Plan (cost-conscious)
- Linux Web App for .NET
- Environment tagging (`environment = dev`)

---

## 📁 Directory Structure

```
infra/
├── envs/
│   └── dev/
│       ├── main.tf          # Terraform resources
│       ├── variables.tf     # Input variable declarations
│       └── dev.tfvars       # Environment-specific values (never commit secrets)
├── README.md                # ← You are here
```

---

🚀 Deployment Options
You can deploy the infrastructure using Terraform either locally or via GitHub Actions CI/CD.

🧪 Local Deployment (Apply from CLI)
Run from the infra/envs/dev/ folder:

bash
Copy
Edit
# Set sensitive variables in your shell environment
export ARM_SUBSCRIPTION_ID="your-subscription-id"
export ARM_TENANT_ID="your-tenant-id"
export ARM_CLIENT_ID="your-app-client-id"
export ARM_CLIENT_SECRET="your-app-client-secret"

# Initialize and apply Terraform
terraform init
terraform plan -var-file=dev.tfvars
terraform apply -var-file=dev.tfvars
✅ This method gives you direct control over infrastructure changes.

🤖 GitHub Actions Deployment (Terraform Plan)
On every push or pull request to main, GitHub will:

Build and test the .NET app

Validate Terraform files

Run terraform plan in the infra/envs/dev folder

Secrets are injected automatically via GitHub's env: block. Ensure the following secrets are configured in your repo:

ARM_SUBSCRIPTION_ID

ARM_TENANT_ID

ARM_CLIENT_ID

ARM_CLIENT_SECRET

📦 Note: CI/CD currently performs terraform plan, not apply. Full automation (auto-apply) can be added later with appropriate approval workflows.


---

## 🔐 Secrets & Sensitive Data

⚠️ Never hardcode or commit sensitive values like subscription ID, tenant ID, or client secrets.

Instead, use GitHub Secrets for CI/CD or `-var` CLI arguments locally:

| Terraform Variable | Source                                          |
| ------------------ | ----------------------------------------------- |
| `subscription_id`  | GitHub Secret: `ARM_SUBSCRIPTION_ID` or CLI var |
| `tenant_id`        | GitHub Secret: `ARM_TENANT_ID` or CLI var       |
| `client_id`        | GitHub Secret: `ARM_CLIENT_ID` or CLI var       |
| `client_secret`    | GitHub Secret: `ARM_CLIENT_SECRET` or CLI var   |

---

## 🧪 Local Usage

From `infra/envs/dev`:

```bash
terraform init

terraform plan \
  -var "subscription_id=..." \
  -var "tenant_id=..." \
  -var "client_id=..." \
  -var "client_secret=..." \
  -var-file=dev.tfvars

terraform apply \
  -var "subscription_id=..." \
  -var "tenant_id=..." \
  -var "client_id=..." \
  -var "client_secret=..." \
  -var-file=dev.tfvars
```

---

## 🤖 GitHub Actions CI/CD

When a commit is pushed to `main` or a pull request is opened:

- .NET project is restored, built, and tested
- Terraform is validated and planned (dev only)
- Secrets are injected securely via `env` block in the CI job

# Ensure your repository has the following secrets set:

- `ARM_SUBSCRIPTION_ID`
- `ARM_TENANT_ID`
- `ARM_CLIENT_ID`
- `ARM_CLIENT_SECRET`

> # 🔐 To enable this, a **Service Principal** must be created and assigned the **Contributor** role on your subscription.

---

## ✅ Dev Environment Tagging

All resources are tagged with:

```hcl
tags = {
  environment = "dev"
}
```

This helps you filter, monitor, and manage costs in Azure.
