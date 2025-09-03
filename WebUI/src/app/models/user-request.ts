export interface UserRequest {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  userName: string;
  password: string;
  roleIds: string[]; // ya number[] agar API me int hai
}
export interface Role {
  id: string;        // ya number use karo agar API number bhejti hai
  name: string;
  description?: string;
}
