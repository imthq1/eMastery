import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import HttpApi from 'i18next-http-backend';
import LanguageDetector from 'i18next-browser-languagedetector';

i18n
    .use(HttpApi)
    .use(LanguageDetector)
    .use(initReactI18next)
    .init({
        supportedLngs: ['en', 'vi'],
        fallbackLng: 'vi',
        defaultNS: 'translation',
        backend: {
            loadPath: '/locales/{{lng}}/{{ns}}.json',
        },
        // debug: import.meta.env.DEV,
    });

export default i18n;