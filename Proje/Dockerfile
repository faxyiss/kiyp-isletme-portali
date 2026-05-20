# 1. AŞAMA: Projeyi Derleme (Build)
FROM node:24-alpine as build-stage
WORKDIR /app

# Bağımlılıkları yükle
COPY package*.json ./
RUN npm install

# Tüm proje dosyalarını kopyala ve derle (dist klasörü oluşur)
COPY . .
RUN npm run build

# 2. AŞAMA: Nginx Sunucusu ile Canlıya Alma
FROM nginx:alpine as production-stage

# Hazırladığımız nginx ayarını içeri aktar
COPY nginx.conf /etc/nginx/conf.d/default.conf

# İlk aşamada derlenen statik dosyaları Nginx'in yayın klasörüne kopyala
COPY --from=build-stage /app/dist /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]