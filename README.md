![CI](https://github.com/aprillnorthcutt/ai-summarizer-azure/actions/workflows/ci.yml/badge.svg)

---

# 🧠 AI-Powered Document Summarizer (Azure + .NET + Terraform)

This project demonstrates how to deliver an intelligent document summarization web app using **Azure Cognitive Services**, a custom **.NET Core API**, and **infrastructure-as-code with Terraform**.  
The infrastructure, CI/CD pipeline, and application code were all developed as part of this portfolio project to showcase hands-on proficiency in **Terraform-based IaC**, **Azure DevOps automation**, and **practical AI integration**.

---

## 💡 Use Case

Designed for consulting-style scenarios where teams need to:

- Automate vendor risk and compliance review  
- Summarize complex documentation using AI  
- Deploy scalable cloud infrastructure using DevOps practices  

> ✅ This use case mirrors real challenges in consulting, GRC, legal tech, and enterprise architecture—extracting actionable insight from dense documents to save time, reduce risk, and improve throughput.

---

## 🔧 Tech Stack

| Layer          | Tools                                      |
|----------------|--------------------------------------------|
| Infra as Code  | Terraform                                  |
| Cloud Platform | Azure                                      |
| AI Services    | Azure Cognitive Services / Azure OpenAI    |
| Backend        | .NET 7 Web API                             |
| Frontend       | Razor Pages *(planned)*                    |
| Storage        | Azure Blob Storage                         |
| Observability  | Azure Application Insights                 |
| CI/CD          | Azure DevOps (pipeline in `/pipelines/`)   |

---

## 📂 Project Structure

```
ai-summarizer-azure/
├── infra/         # Terraform modules for all Azure infra
├── src/           # .NET API (in progress)
├── pipelines/     # CI pipeline (Terraform deploy)
├── docs/          # Diagrams, design docs
├── .gitignore
└── README.md
```

---

## 🚀 Manual Deployment (Terraform)

To deploy infrastructure locally:

```bash
cd infra
terraform init
terraform apply
```

To tear down and avoid Azure costs:

```bash
terraform destroy
```

---

## 🔄 CI/CD Pipeline

A sample Terraform deployment pipeline is provided here:  
[**pipelines/azure-pipeline.yml**](https://github.com/aprillnorthcutt/ai-summarizer-azure/blob/main/pipelines/azure-pipeline.yml)

To use it:

- Connect this repo to Azure DevOps  
- Configure a pipeline using the YAML file above  

ℹ️ While infrastructure is deployed manually during development to reduce cloud spend, this pipeline demonstrates enterprise-ready automation using Terraform in Azure DevOps.

---

## 🧪 In Progress

- ✅ Terraform IaC modules  
- ✅ Azure DevOps deployment pipeline  
- ⏳ .NET backend API to upload PDFs and call Azure AI  
- ⏳ UI to visualize the summary output  
- ⏳ Architecture diagram (`docs/system-architecture.drawio`)  
- ⏳ **Optional live deployment on Azure App Service (will be added temporarily for demo or review purposes)**
  
---
![Hands-on Infrastructure, CI/CD, and App by April Northcutt](https://img.shields.io/badge/built%20and%20coded%20by-April%20Northcutt-blueviolet)

## 👋 About the Author

Hi, I’m **April Northcutt**, a Software Engineering Manager with deep experience in Azure modernization, DevOps, and delivery leadership.  
Prior to moving into platform and delivery strategy roles, I spent over a decade focused on backend and full-stack engineering using the .NET tech stack — including C#, ASP.NET, SQL Server, and modern DevOps practices.  
I also specialize in uncovering process inefficiencies and optimizing technical workflows, driving measurable impact across development, infrastructure, and team operations.  

This project demonstrates applied AI delivery patterns that augment real-world cloud solutions.

> *Note: My LinkedIn reflects only a portion of my consulting and engineering background.*

🔗 [LinkedIn](https://www.linkedin.com/in/aprillnorthcutt) | [GitHub](https://github.com/aprillnorthcutt)

### 🏅 Certifications

- Microsoft Certified: Azure Developer Associate (AZ-204)  
- Microsoft Certified: Azure Administrator Associate (AZ-104)  
- Microsoft Certified: Azure Fundamentals (AZ-900)  
- Microsoft Certified: AI Fundamentals (AI-900)  
- Microsoft Certified: Microsoft 365 Fundamentals (MS-900)  
- ICAgile Certified Professional (ICP)  
- Certified Scrum Developer – ASD (via Avanade & Scrum.org partnership)  
- Green Software for Practitioners (LFC131)



---

## 📄 License

MIT – feel free to fork and adapt.
