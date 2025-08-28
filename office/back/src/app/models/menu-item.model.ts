export interface MenuItem {
  label: string;
  key: string;
  active?: boolean;
  route?: string;
  icon?: string;
  submenu?: MenuItem[];
}