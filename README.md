# PropagaMed

> Aplicativo mobile multiplataforma para **propagandistas farmacêuticos** gerenciarem médicos, agendarem visitas e acompanharem todo o ciclo de relacionamento com a classe médica.

---

## 📱 Sobre o Projeto

O **PropagaMed** foi desenvolvido para resolver um problema real do dia a dia do propagandista médico: o controle manual e fragmentado de informações sobre médicos visitados, datas de visitas, preferências de atendimento e histórico de relacionamento.

Com o PropagaMed, tudo fica centralizado em um único aplicativo — disponível para **Android** e **iOS**.

---

## ✨ Funcionalidades

### 👨‍⚕️ Gestão de Médicos
- Cadastro completo: nome, especialidade, CRM, endereço, CEP, e-mail, telefone e celular
- Registro de secretário(a) de contato
- Preferências de visita: tipo (presencial ou on-line) e dias/turnos preferidos (manhã, tarde, noite)
- Filtro por localização (Rio de Janeiro / Niterói)
- Busca em tempo real por nome
- Exportação de **cartão virtual** do médico (seleção de até 2 médicos para comparação)

### 📅 Gestão de Visitas
- Agendamento de visitas com data e horário
- Seleção do tipo de visita (presencial ou on-line)
- Campo de observação por visita
- Indicação visual de **aniversário** do médico na listagem de visitas
- Exportação de visitas (em CSV ou relatório)
- Histórico completo e busca por médico
- Opção de deletar visitas individualmente ou em lote

### 🔐 Autenticação
- Tela de login com e-mail e PIN de 4 dígitos
- Recuperação de banco de dados local

---

## 🛠️ Tecnologias Utilizadas

| Tecnologia | Finalidade |
|---|---|
| **Xamarin.Forms** | Framework multiplataforma Android/iOS |
| **C# / .NET** | Linguagem e runtime principal |
| **SQLite** | Banco de dados local no dispositivo |
| **Xamarin.CommunityToolkit** | Máscaras de input (telefone, celular) |
| **MVVM** | Padrão de arquitetura (ViewModel + Bindings) |

---

## 🏗️ Arquitetura

```
PropagaMed/
├── Model/              # Entidades: Medico, Visita, Usuario
├── ViewModel/          # ViewModels com lógica de negócio e bindings
├── View/               # Pages XAML (Login, Home, MedicoView, VisitaView)
├── Service/            # Serviços de acesso a dados (SQLite)
└── Helpers/            # Utilitários e helpers de exportação
```

O projeto segue o padrão **MVVM** nativo do Xamarin.Forms, com `INotifyPropertyChanged` e data bindings declarados no XAML.

---

## 📲 Telas Principais

### Login
Autenticação por e-mail e PIN (4 dígitos). Possui botão de recuperação do banco de dados local para situações de troca de dispositivo.

### Home (TabbedPage)
Interface principal com 4 abas na barra inferior:

1. **Nova Visita** — formulário para agendamento de visita com seleção de médico, data, horário e tipo
2. **Novo Médico** — formulário de cadastro com informações completas e preferências de visita por dia/turno
3. **Visitas** — lista de visitas com busca, opções de detalhar, observar e deletar; exportação de relatório
4. **Médicos** — lista de médicos com filtro por localização, busca, modo de exportação de cartões e acesso às visitas por médico

---

## 🚀 Como Executar

### Pré-requisitos
- [Visual Studio 2022](https://visualstudio.microsoft.com/) com workload **Mobile development with .NET**
- SDK do Android (API 21+) e/ou Xcode (iOS 13+)
- Emulador Android ou dispositivo físico

### Passos

```bash
# 1. Clone o repositório
git clone https://github.com/luidioliveira/PropagaMed.git

# 2. Abra a solution no Visual Studio
# PropagaMed/PropagaMed.sln

# 3. Restaure os pacotes NuGet
# Tools > NuGet Package Manager > Restore

# 4. Selecione o projeto de destino (Android ou iOS) e execute
```

> **Nota:** O banco de dados SQLite é criado automaticamente na primeira execução no diretório de dados do aplicativo no dispositivo.

---

## 📦 Dependências NuGet

| Pacote | Versão |
|---|---|
| Xamarin.Forms | 5.x |
| Xamarin.Essentials | 1.x |
| Xamarin.CommunityToolkit | 2.x |
| sqlite-net-pcl | 1.x |

---

## 🗺️ Roadmap

- [ ] Migração para **.NET MAUI** (sucessor oficial do Xamarin.Forms)
- [ ] Sincronização em nuvem (backup remoto)
- [ ] Dashboard com métricas de visitas por período
- [ ] Notificações push para lembretes de visita
- [ ] Suporte a múltiplos territórios/regiões além de RJ/Niterói

---

## 🤝 Contribuindo

Contribuições são bem-vindas! Sinta-se à vontade para abrir uma *issue* ou *pull request*.

1. Faça um **fork** do projeto
2. Crie uma branch: `git checkout -b feature/minha-feature`
3. Commit suas alterações: `git commit -m 'feat: adiciona minha feature'`
4. Push para a branch: `git push origin feature/minha-feature`
5. Abra um **Pull Request**

---

## 📄 Licença

Distribuído sob a licença MIT. Veja `LICENSE` para mais informações.

---

<p align="center">
  Desenvolvido com ❤️ para facilitar o dia a dia do propagandista farmacêutico.
</p>
