#!/bin/bash
# filepath: /home/john/Bureau/PROJECTS/MJ-SOLUTIONS/install.sh

# Vérification des dépendances
for cmd in dialog jq openssl; do
    if ! command -v "$cmd" &> /dev/null; then
        echo -e "\e[31mErreur : $cmd n'est pas installé. Installez-le avec 'sudo apt install $cmd'\e[0m"
        exit 1
    fi
done

# Couleurs
RED="\Z1"
GREEN="\Z2"
YELLOW="\Z3"
BLUE="\Z4"
NORMAL="\Zn"

function info_box() {
    dialog --colors --title "$1" --msgbox "$2" 8 60
}

dialog --colors --title "${BLUE}MJ-SOLUTIONS - Installation${NORMAL}" --msgbox "${GREEN}Bienvenue dans le script d'installation MJ-SOLUTIONS !${NORMAL}\n\nCe script va vous guider étape par étape." 10 60

install_choice=$(dialog --colors --title "${YELLOW}Installation${NORMAL}" --menu "Que souhaitez-vous installer ?" 15 60 4 \
    1 "${GREEN}Tout le repo${NORMAL}" \
    2 "${BLUE}Sélectionner (API, Backend, Frontend)${NORMAL}" \
    3 "${YELLOW}Créer un nouvel utilisateur${NORMAL}" \
    2>&1 >/dev/tty)

if [[ "$install_choice" == "3" ]]; then
    # SECTION CREATION USER UNIQUEMENT
    firstname=$(dialog --colors --inputbox "${YELLOW}Prénom${NORMAL}" 8 60 "" 2>&1 >/dev/tty)
    lastname=$(dialog --colors --inputbox "${YELLOW}Nom${NORMAL}" 8 60 "" 2>&1 >/dev/tty)
    email=$(dialog --colors --inputbox "${YELLOW}Email${NORMAL}" 8 60 "" 2>&1 >/dev/tty)
    password=$(dialog --colors --insecure --passwordbox "${YELLOW}Mot de passe${NORMAL}" 8 60 "" 2>&1 >/dev/tty)

    role=$(dialog --colors --title "${YELLOW}Rôle utilisateur${NORMAL}" --menu "Sélectionnez le rôle :" 10 60 3 \
        "SUPER_ADMIN" "${GREEN}Super administrateur${NORMAL}" \
        "ADMIN" "${BLUE}Administrateur${NORMAL}" \
        "USER" "${YELLOW}Utilisateur${NORMAL}" \
        2>&1 >/dev/tty)

    permissions=("CREATE_USER" "READ_USER" "UPDATE_USER" "DELETE_USER" "CREATE_SKILL" "READ_SKILL" "UPDATE_SKILL" "DELETE_SKILL" "CREATE_PROJECT" "READ_PROJECT" "UPDATE_PROJECT" "DELETE_PROJECT")
    perm_choices=$(dialog --colors --checklist "Sélectionnez les permissions :" 20 60 12 \
        1 "CREATE_USER" off \
        2 "READ_USER" off \
        3 "UPDATE_USER" off \
        4 "DELETE_USER" off \
        5 "CREATE_SKILL" off \
        6 "READ_SKILL" off \
        7 "UPDATE_SKILL" off \
        8 "DELETE_SKILL" off \
        9 "CREATE_PROJECT" off \
        10 "READ_PROJECT" off \
        11 "UPDATE_PROJECT" off \
        12 "DELETE_PROJECT" off \
        2>&1 >/dev/tty)

    selected_permissions=()
    for idx in $perm_choices; do
        idx=$(echo "$idx" | tr -d '"')
        selected_permissions+=("\"${permissions[$((idx-1))]}\"")
    done
    perms_json=$(printf ",%s" "${selected_permissions[@]}")
    perms_json="[${perms_json:1}]"

    curl -X POST http://localhost:5187/api/auth/register \
        -H "Content-Type: application/json" \
        -d "{\"firstname\":\"$firstname\",\"lastname\":\"$lastname\",\"email\":\"$email\",\"password\":\"$password\",\"role\":\"$role\",\"permissions\":$perms_json}"
    info_box "Utilisateur" "${GREEN}Utilisateur créé (si l'API est démarrée et le endpoint existe).${NORMAL}"
    clear
    exit 0
fi

# --- Parcours installation classique ---
if [[ "$install_choice" == "1" ]]; then
    info_box "Installation" "${GREEN}Installation complète du repo...${NORMAL}"

    info_box "API" "${BLUE}Installation de l'API (.NET)...${NORMAL}"
    cd api
    dotnet restore
    dotnet build
    dotnet ef database update
    cd ..

    if [ -d "backend" ]; then
        info_box "Backend" "${BLUE}Installation du backend (Angular)...${NORMAL}"
        cd backend
        npm install
        ng build
        cd ..
    fi

    if [ -d "frontend" ]; then
        info_box "Frontend" "${BLUE}Installation du frontend (React)...${NORMAL}"
        cd frontend
        npm install
        npm run build
        cd ..
    fi

elif [[ "$install_choice" == "2" ]]; then
    parts=$(dialog --colors --title "${YELLOW}Sélection des modules${NORMAL}" --checklist "Sélectionnez les parties à installer :" 15 60 3 \
        "API" "${BLUE}API (.NET)${NORMAL}" on \
        "Backend" "${BLUE}Backend (Angular)${NORMAL}" off \
        "Frontend" "${BLUE}Frontend (React)${NORMAL}" off \
        2>&1 >/dev/tty)
    for part in $parts; do
        case $part in
            "\"API\"")
                info_box "API" "${BLUE}Installation de l'API (.NET)...${NORMAL}"
                cd api
                dotnet restore
                dotnet build
                dotnet ef database update
                cd ..
                ;;
            "\"Backend\"")
                if [ -d "backend" ]; then
                    info_box "Backend" "${BLUE}Installation du backend (Angular)...${NORMAL}"
                    cd backend
                    npm install
                    ng build
                    cd ..
                else
                    info_box "Backend" "${RED}Répertoire backend non trouvé.${NORMAL}"
                fi
                ;;
            "\"Frontend\"")
                if [ -d "frontend" ]; then
                    info_box "Frontend" "${BLUE}Installation du frontend (React)...${NORMAL}"
                    cd frontend
                    npm install
                    npm run build
                    cd ..
                else
                    info_box "Frontend" "${RED}Répertoire frontend non trouvé.${NORMAL}"
                fi
                ;;
        esac
    done
fi

# --- Configuration BDD ---
db_name=$(dialog --colors --inputbox "${YELLOW}Nom de la base de données${NORMAL} (défaut: mj_solutions)" 8 60 "mj_solutions" 2>&1 >/dev/tty)
db_user=$(dialog --colors --inputbox "${YELLOW}Utilisateur MySQL${NORMAL} (défaut: root)" 8 60 "root" 2>&1 >/dev/tty)
db_pass=$(dialog --colors --insecure --passwordbox "${YELLOW}Mot de passe MySQL${NORMAL} (défaut: root)" 8 60 "root" 2>&1 >/dev/tty)
db_host=$(dialog --colors --inputbox "${YELLOW}Hôte MySQL${NORMAL} (défaut: localhost)" 8 60 "localhost" 2>&1 >/dev/tty)

env_choice=$(dialog --colors --title "${YELLOW}Environnement${NORMAL}" --menu "Environnement de configuration ?" 10 60 2 \
    1 "${GREEN}Développement${NORMAL}" \
    2 "${RED}Production${NORMAL}" \
    2>&1 >/dev/tty)

if [[ "$env_choice" == "2" ]]; then
    config_file="api/appsettings.Production.json"
else
    config_file="api/appsettings.Development.json"
fi

if [[ ! -f "$config_file" ]]; then
    info_box "Configuration" "${YELLOW}Fichier $config_file non trouvé, création...${NORMAL}"
    cat <<EOF > "$config_file"
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DatabaseConnection": ""
  },
  "Jwt": {
    "Key": "",
    "Issuer": ""
  },
  "Cors": {
    "AllowedOrigins": [ "*" ]
  },
  "AES": {
    "Key": "",
    "IV": ""
  }
}
EOF
fi

jwt_key=$(dialog --colors --inputbox "${YELLOW}Clé JWT${NORMAL} (défaut: générée aléatoirement)" 8 60 "$(openssl rand -hex 32)" 2>&1 >/dev/tty)
jwt_issuer=$(dialog --colors --inputbox "${YELLOW}Issuer JWT${NORMAL} (défaut: mj_solutions)" 8 60 "mj_solutions" 2>&1 >/dev/tty)
aes_key=$(dialog --colors --inputbox "${YELLOW}Clé AES${NORMAL} (défaut: générée aléatoirement)" 8 60 "$(openssl rand -hex 16)" 2>&1 >/dev/tty)
aes_iv=$(dialog --colors --inputbox "${YELLOW}IV AES${NORMAL} (défaut: généré aléatoirement)" 8 60 "$(openssl rand -hex 16)" 2>&1 >/dev/tty)
cors_origins=$(dialog --colors --inputbox "${YELLOW}Origines CORS autorisées${NORMAL} (séparées par des virgules, * pour tout autoriser)" 8 60 "*" 2>&1 >/dev/tty)
# Generate keys internally
gen_jwt_key=$(openssl rand -hex 32)
gen_aes_key=$(openssl rand -hex 16)
gen_aes_iv=$(openssl rand -hex 16)

jwt_key=$(dialog --colors --inputbox "${YELLOW}Clé JWT${NORMAL} (laissez vide pour générer automatiquement)" 8 60 "" 2>&1 >/dev/tty)
jwt_issuer=$(dialog --colors --inputbox "${YELLOW}Issuer JWT${NORMAL} (défaut: mj_solutions)" 8 60 "mj_solutions" 2>&1 >/dev/tty)
aes_key=$(dialog --colors --inputbox "${YELLOW}Clé AES${NORMAL} (laissez vide pour générer automatiquement)" 8 60 "" 2>&1 >/dev/tty)
aes_iv=$(dialog --colors --inputbox "${YELLOW}IV AES${NORMAL} (laissez vide pour générer automatiquement)" 8 60 "" 2>&1 >/dev/tty)
cors_origins=$(dialog --colors --inputbox "${YELLOW}Origines CORS autorisées${NORMAL} (séparées par des virgules, * pour tout autoriser)" 8 60 "*" 2>&1 >/dev/tty)

# Use generated keys if user input is empty
if [[ -z "$jwt_key" ]]; then
    jwt_key="$gen_jwt_key"
fi
if [[ -z "$aes_key" ]]; then
    aes_key="$gen_aes_key"
fi
if [[ -z "$aes_iv" ]]; then
    aes_iv="$gen_aes_iv"
fi
db_conn="server=$db_host;database=$db_name;user=$db_user;password=$db_pass"
cors_json=$(echo "$cors_origins" | awk -F, '{printf "[\"%s\"", $1; for(i=2;i<=NF;i++) printf ", \"%s\"", $i; print "]"}')

tmp_file=$(mktemp)
jq ".ConnectionStrings.DatabaseConnection = \"$db_conn\" | \
    .Jwt.Key = \"$jwt_key\" | \
    .Jwt.Issuer = \"$jwt_issuer\" | \
    .AES.Key = \"$aes_key\" | \
    .AES.IV = \"$aes_iv\" | \
    .Cors.AllowedOrigins = $cors_json" "$config_file" > "$tmp_file" && mv "$tmp_file" "$config_file"

info_box "Configuration" "${GREEN}Configuration mise à jour dans $config_file${NORMAL}"

dialog --colors --title "${GREEN}Installation terminée${NORMAL}" --msgbox "${GREEN}Installation terminée !${NORMAL}\n\nVous pouvez maintenant démarrer vos services." 10 60

clear