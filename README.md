![CI .Net Status](https://github.com/aprillnorthcutt/ai-summarizer-azure/actions/workflows/ci-dotnet.yml/badge.svg)
![CI Terraform Status](https://github.com/aprillnorthcutt/ai-summarizer-azure/actions/workflows/ci-terraform.yml/badge.svg)
![Status: In Progress](https://img.shields.io/badge/status-in--progress-yellow)
![Terraform Version](https://img.shields.io/badge/Terraform-1.6.6-blue)

---

# 🧠 AI-Powered Document Summarizer (Azure + .NET + Terraform)

This portfolio project demonstrates how to deliver an intelligent document summarization web app using **Azure Cognitive Services**, a custom **.NET Core API**, and **infrastructure-as-code with Terraform**.

The infrastructure, CI/CD pipeline, and application code were all developed by me to showcase hands-on proficiency in **Terraform-based IaC**, **Azure DevOps automation**, and **practical AI integration** — simulating how modern delivery teams build scalable, cloud-native solutions.

---

## 💡 Use Case

Designed for consulting-style scenarios where teams need to:

- Automate vendor risk and compliance review  
- Summarize complex documentation using AI  
- Deploy scalable cloud infrastructure using DevOps best practices  

> ✅ This use case mirrors real challenges in consulting, GRC, legal tech, and enterprise architecture—extracting actionable insights from dense documents to save time, reduce risk, and improve throughput.

---

## 🔧 Tech Stack

| Layer            | Tools                                           |
|------------------|--------------------------------------------------|
| Infra as Code    | Terraform (modular structure, remote backend)   |
| Cloud Platform   | Azure (App Service, Cognitive Services, Storage)|
| AI Services      | Azure Cognitive Services / Azure OpenAI         |
| Backend API      | .NET 7 Web API                                  |
| Frontend         | Razor Pages *(planned enhancement)*             |
| Observability    | Azure Application Insights                      |
| CI/CD            | GitHub Actions & Azure DevOps                   |

---

## 📂 Project Structure

```text
ai-summarizer-azure/
├── infra/             # Terraform modules and environments (dev/test/prod)
│   └── envs/
├── src/               # .NET API and test project
├── pipelines/         # Azure DevOps pipeline definitions
├── .github/workflows/ # GitHub Actions (CI/CD)
├── docs/              # Diagrams, technical journey docs
├── .gitignore
└── README.md
```

---

## 📦 Infrastructure Deployment (Terraform + Azure)

You can deploy infrastructure either locally or via GitHub Actions:

- 💻 **Local CLI**: Use `terraform init` and `terraform apply` with environment-specific tfvars  
- ☁️ **GitHub Actions**: On push or PR, run workflows defined in `.github/workflows`

📁 [View the full deployment guide](docs/Deployment-guide.md)

---

## 🔄 CI/CD Pipeline

### ✅ GitHub Actions
- Auto-plan/apply Terraform from PRs or commits  
- Secrets managed via GitHub repository settings 
- Split into 2 files for maintaiability between Terraform and App code. 
- [View CI .Net workflow file](.github/workflows/ci-dotnet.yml)
- [View CI Terraform workflow file](.github/workflows/ci-terraform.yml)

### ✅ Azure DevOps (In Progress)
- [pipelines/azure-pipeline.yml](pipelines/azure-pipeline.yml) provides a sample deploy pipeline  
- Can be imported directly into Azure DevOps

> ℹ️ Infra is deployed manually during development to reduce spend, but both CI/CD options simulate enterprise-ready workflows.

---

## 📈 What This Project Demonstrates

- Delivery and CI/CD strategy using GitHub Actions and Azure DevOps
- Secure, modular infrastructure using Terraform and service principals
- Real-world use case simulating document AI automation
- Leadership in technical planning, tooling, and architecture decisions

---

## 📘 Project Journey Docs

- [CI/CD + GitHub Flow](docs/GIT-GITHUB-CICD-JOURNEY.png)
- [Azure Infrastructure Setup](docs/AZURE-INFRA-SETUP.png)
- [Deployment Guide](docs/Deployment-guide.md)
- [Architecture Diagram](docs/Azure%20Local%20Setup.png)

---

## 📈 Project Status

| Feature                    | Status   |
|----------------------------|----------|
| Terraform Infra Modules    | ✅ Done  |
| GitHub Actions CI/CD       | ✅ Done  |
| Azure DevOps Pipelines     | ⏳ In Progress |
| .NET API (PDF Upload + AI) | ⏳ In Progress |
| Frontend UI (optional)     | ⏳ Planned |
| Architecture Diagram       | ⏳ In Progress |
| Live Demo                  | 🔜 Optional (for review/demo)

---

## 👩‍💻 About the Author

Hi, I’m **April Northcutt**, a Software Engineering Manager with deep experience in Azure modernization, DevOps, and delivery leadership.

Before transitioning into platform strategy and technical leadership roles, I spent over a decade building backend and full-stack applications using C#, ASP.NET, SQL Server, and modern DevOps practices.

I specialize in uncovering inefficiencies, automating processes, and aligning technical delivery with business outcomes.

> *Note: My LinkedIn reflects only a portion of my consulting and engineering background.*

🔗 [LinkedIn](https://www.linkedin.com/in/aprillnorthcutt) | [GitHub](https://github.com/aprillnorthcutt)

---

## 🏅 Certifications

✅ Azure Developer Associate (AZ-204)  
✅ Azure Administrator Associate (AZ-104)  
✅ AI Fundamentals (AI-900)  
✅ Microsoft 365 Fundamentals (MS-900)  
✅ Azure Fundamentals (AZ-900)  
✅ ICAgile Certified Professional (ICP)  
✅ Certified Scrum Developer – ASD (Avanade/Scrum.org)  
✅ Green Software for Practitioners (LFC131)

---

## 📄 License

MIT – feel free to fork or adapt for your own DevOps learning.