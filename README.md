# 🧠 AI-Powered Document Summarizer – Azure, .NET, and Terraform

🔗 **Live Demo**: [KeywordVista App](https://keywordvista.azurewebsites.net)  
🌐 **AI Tools Landing Page**: [AI Tools Hub](https://ai-tools-hub.azurewebsites.net)

---

## Overview

This project showcases an end-to-end AI-powered summarization solution built using **Azure Cognitive Services**, **.NET Web API**, and **Terraform-based infrastructure as code**. It was created as part of a technical portfolio to demonstrate delivery leadership, DevOps maturity, and applied AI in a cloud-native environment.

It simulates real-world use cases where organizations must extract insight from complex documents for compliance, vendor risk review, or operational efficiency.

---

## 🎯 Purpose

- **For Delivery Teams**: Showcases scalable infrastructure, DevOps automation, and cloud-native development.
- **For Recruiters**: Demonstrates Azure expertise, Terraform, CI/CD pipelines, and full-stack delivery.
- **For Clients/Stakeholders**: Illustrates how AI can streamline document processing and surface key insights. 

---

## 🛠️ Technologies Used

| Layer            | Tools                                           |
|------------------|--------------------------------------------------|
| Infrastructure   | Terraform, Azure CLI                            |
| Cloud Services   | Azure App Service, Cognitive Services, Storage  |
| API Backend      | .NET 7 Web API                                  |
| AI Models        | Azure OpenAI, Azure Language Services           |
| CI/CD            | GitHub Actions, Azure DevOps(in progress)       |
| Monitoring       | Azure Application Insights                      |
| Frontend         | React + Vite *(earlier Razor Pages plan dropped)*

---

## 🧱 Architecture & Layout

```
ai-summarizer-azure/
├── infra/             # Terraform modules and environments
│   └── envs/
├── src/               # .NET API and tests
├── pipelines/         # Azure DevOps YAML pipeline
├── .github/           # GitHub Actions workflows
├── docs/              # Diagrams and guides
└── README.md
```

---

## 📦 Project Features

| Feature                              | Status           |
|--------------------------------------|------------------|
| Modular Terraform IaC                | ✅ Complete       |
| Azure Web App + Cognitive Services   | ✅ Complete       |
| Extractive + Abstractive Summarizer  | ✅ Complete       |
| GitHub Actions CI/CD (infra + app)   | ✅ Complete       |
| Azure DevOps YAML pipeline           | ⏳ In Progress    |
| React Frontend UI                    | ✅ Complete       |
| Live Demo Hosted on Azure            | ✅ Live Now       |

---

## 🖥️ Demo Example

The web app allows users to:

- Paste raw text or upload a document (PDF, PNG, DOCX)
- Select **Extractive** (sentence selection) or **Abstractive** (rephrased summary) mode
- View the AI-generated summary and key phrases
- Explore front-end UX backed by real Azure services

---

## 📘 Sample Architecture & Pipelines

| Resource              | Link |
|-----------------------|------|
| Deployment Guide      | `docs/Deployment-guide.md` |
| CI/CD Architecture    | `docs/GIT-GITHUB-CICD-JOURNEY.png` |
| Infra Architecture    | `docs/AZURE-INFRA-SETUP.png` |
| System Diagram        | `docs/Azure Local Setup.png` |

---

## 🧠 Strategic Commentary

This project simulates enterprise cloud delivery—modularized IaC, backend AI orchestration, frontend UX integration, and full CI/CD pipelines. It demonstrates not only code execution but real-world DevOps habits such as environment separation, observability, and progressive rollout readiness.

It’s structured to mirror how engineering leaders plan, execute, and optimize platforms across cloud and AI initiatives.

---

## 🚀 Future Enhancements

- Terraform deployment via Azure DevOps pipelines  
- Expand summarizer with translation & sentiment analysis  
- Add document download & preview features  
- Apply Azure RBAC & identity controls to API endpoints  

---

## 👤 About the Creator

**April Northcutt**  
Hi, I’m April Northcutt, a Software Engineering Manager with deep experience in Azure modernization, DevOps, and delivery leadership.

Before transitioning into platform strategy and technical leadership roles, I spent over a decade building backend and full-stack applications using C#, ASP.NET, SQL Server, and modern DevOps practices.

I specialize in uncovering inefficiencies, automating processes, and aligning technical delivery with business outcomes.

> Note: My LinkedIn reflects only a portion of my consulting and engineering background.

🏅 **Certifications**  
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

MIT License.  
You are free to adapt, extend, and integrate this work into your own delivery workflows.