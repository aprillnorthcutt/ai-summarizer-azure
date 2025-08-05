# 🛠️ Infrastructure as Code (Terraform + Azure)

This folder contains Terraform configuration to deploy the infrastructure needed for the AI-powered document summarizer. It provisions:

- Azure Resource Group
- Linux App Service Plan (cost-conscious)
- Linux Web App for .NET
- Environment tagging (`environment = dev`)

---

## 📁 Directory Structure

```bash
infra/
├── envs/
│   └── dev/
│       ├── main.tf          # Terraform resources
│       ├── variables.tf     # Input variable declarations
│       └── dev.tfvars       # Environment-specific values (never commit secrets)
├── README.md                # ← You are here
```

---

## 🔐 Secrets & Sensitive Data

⚠️ Never hardcode or commit sensitive values like subscription ID, tenant ID, or client secrets.

Instead, **use GitHub Secrets** for CI/CD or `-var` CLI arguments locally:

| Terraform Variable   | Source                   |
|----------------------|--------------------------|
| `subscription_id`    | GitHub Secret: `ARM_SUBSCRIPTION_ID` or CLI var |
| `tenant_id`          | GitHub Secret: `ARM_TENANT_ID` or CLI var |
| `client_id`          | GitHub Secret: `ARM_CLIENT_ID` or CLI var |
| `client_secret`      | GitHub Secret: `ARM_CLIENT_SECRET` or CLI var |

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

- `.NET project` is restored, built, and tested
- `Terraform` is validated and planned (dev only)
- Secrets are injected securely via `env` block in the CI job

Ensure your repository has the following secrets set:

```plaintext
ARM_SUBSCRIPTION_ID
ARM_TENANT_ID
ARM_CLIENT_ID
ARM_CLIENT_SECRET
```

---

## ✅ Dev Environment Tagging

All resources are tagged with:

```hcl
tags = {
  environment = "dev"
}
```

This helps you filter, monitor, and manage costs in Azure.