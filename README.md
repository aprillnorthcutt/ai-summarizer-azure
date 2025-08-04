# 🧠 AI-Powered Document Summarizer (Azure + .NET + Terraform)

This project demonstrates how to deliver an intelligent document summarization web app using **Azure Cognitive Services** (or Azure OpenAI), a **.NET Core API**, and **infrastructure-as-code with Terraform**. It simulates a real-world solution to extract and summarize long compliance documents (e.g., SOC2 reports, vendor security forms).

---

## 💡 Use Case

Designed for consulting-style scenarios where teams need to:

- Automate vendor risk and compliance review
- Summarize complex documentation using AI
- Deploy scalable cloud infrastructure using DevOps practices

---

## 🔧 Tech Stack

| Layer        | Tools |
|--------------|-------|
| Infra as Code | Terraform |
| Cloud Platform | Azure |
| AI Services | Azure Cognitive Services / Azure OpenAI |
| Backend | .NET 7 Web API |
| Frontend | Razor Pages (planned) |
| Storage | Azure Blob |
| Observability | Azure App Insights |
| CI/CD | Azure DevOps (pipeline in `pipelines/`) |

---

## 📂 Project Structure
```
ai-summarizer-azure/
├── infra/ # Terraform modules for all Azure infra
├── src/ # .NET API (in progress)
├── pipelines/ # CI pipeline (Terraform deploy)
├── docs/ # Diagrams, design docs (optional)
├── .gitignore
├── README.md
```

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

🔄 CI/CD Pipeline

A Terraform pipeline is available in:

pipelines/terraform-deploy.yml

To use it:

Connect this repo to Azure DevOps

Configure a pipeline using the above path

Manual terraform apply is currently used during development for cost control. The pipeline demonstrates automated deployment best practices.


---

🧪 In Progress
✅ Terraform IaC modules

✅ Azure DevOps deployment pipeline

⏳ .NET backend API to upload PDFs and call Azure AI

⏳ UI to visualize the summary output

⏳ Diagram of architecture (docs/system-architecture.drawio)

👋 About the Author
Hi, I’m April Northcutt, a Software Engineering Manager with deep experience in Azure modernization, DevOps, and delivery leadership. This project demonstrates applied AI delivery patterns that augment real-world cloud solutions.







📄 License
MIT – feel free to fork and adapt.

---

Let me know if you want a downloadable `.md` file or to add new sections later like:
- Screenshots
- Demo video
- Live URL
- Architecture PNG diagram









Ask ChatGPT
